using Stoat.Core.Image.Leases;
using Stoat.Core.Interfaces.Caching;
using Stoat.Core.Interfaces.Decode;
using Stoat.Core.Interfaces.Leases;

namespace Stoat.Core.Image.Caching;

public sealed class MemoryImageCache : IImageMemoryCache {
    private readonly object _gate = new();
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly PriorityQueue<(string Key, Entry Entry), long> _evictionQueue = new();
    private readonly MemoryImageCacheOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ITimer? _cleanupTimer;
    private long _accessSequence;
    private uint _loadedItemCount;
    private bool _disposed;

    public MemoryImageCache(MemoryImageCacheOptions? options = null)
        : this(options, TimeProvider.System) {
    }

    internal MemoryImageCache(MemoryImageCacheOptions? options, TimeProvider timeProvider) {
        _options = options ?? new MemoryImageCacheOptions();
        _options.Validate();
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        if (_options.AbsoluteExpiration is not null || _options.SlidingExpiration is not null) {
            var period = GetCleanupPeriod();
            _cleanupTimer = _timeProvider.CreateTimer(CleanupExpiredEntries, null, period, period);
        }
    }

    public async Task<IImageLease?> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<IImage?>> factory,
        CancellationToken cancellationToken = default) {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be empty.", nameof(key));
        ArgumentNullException.ThrowIfNull(factory);

        Entry entry;
        Task<IImage?> loadingTask;
        var startLoading = false;
        lock (_gate) {
            ThrowIfDisposed();

            if (!_entries.TryGetValue(key, out entry!)) {
                entry = new Entry();
                _entries.Add(key, entry);
            }

            if (entry.Image is not null) {
                if (entry.LeaseCount == 0 && IsExpired(entry))
                    DetachEntryLocked(key, entry);
                else {
                    Touch(key, entry);
                    entry.LeaseCount++;
                    return new MemoryImageLease(entry.Image, () => Release(key, entry));
                }
            }

            if (!_entries.TryGetValue(key, out entry!)) {
                entry = new Entry();
                _entries.Add(key, entry);
            }

            if (entry.LoadingTask is null) {
                entry.Completion = new TaskCompletionSource<IImage?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                entry.LoadingTask = entry.Completion.Task;
                startLoading = true;
            }

            entry.WaiterCount++;
            loadingTask = entry.LoadingTask;
        }

        if (startLoading)
            _ = LoadEntryAsync(key, entry, factory);

        IImage? image;
        try {
            image = await loadingTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch {
            lock (_gate) {
                entry.WaiterCount--;
                EnqueueForEvictionIfEligible(key, entry);
                DisposeDetachedEntryIfUnused(entry);
            }

            throw;
        }

        lock (_gate) {
            entry.WaiterCount--;
            if (_disposed) {
                DisposeDetachedEntryIfUnused(entry);
                return null;
            }

            if (image is null) {
                DetachEntryLocked(key, entry);
                return null;
            }

            entry.LeaseCount++;
            return new MemoryImageLease(image, () => Release(key, entry));
        }
    }

    private async Task LoadEntryAsync(
        string key,
        Entry entry,
        Func<CancellationToken, Task<IImage?>> factory) {
        var completion = entry.Completion!;
        try {
            var image = await factory(CancellationToken.None).ConfigureAwait(false);
            lock (_gate) {
                entry.LoadingTask = null;
                entry.Completion = null;
                entry.Image = image;
                if (image is not null) {
                    entry.StrongSince = _timeProvider.GetUtcNow();
                    entry.LastAccess = entry.StrongSince;
                    entry.LastAccessSequence = ++_accessSequence;
                    _loadedItemCount++;
                    EnqueueForEvictionIfEligible(key, entry);
                    EnforceMaxItemsLocked();
                }
                else {
                    DetachEntryLocked(key, entry);
                }

                DisposeDetachedEntryIfUnused(entry);
            }

            completion.TrySetResult(image);
        }
        catch (Exception e) {
            lock (_gate) {
                entry.LoadingTask = null;
                entry.Completion = null;
                DetachEntryLocked(key, entry);
            }

            completion.TrySetException(e);
        }
    }

    public void Clear() {
        lock (_gate) {
            foreach (var pair in new List<KeyValuePair<string, Entry>>(_entries)) {
                DetachEntryLocked(pair.Key, pair.Value);
            }
        }
    }

    public void Dispose() {
        lock (_gate) {
            if (_disposed)
                return;

            _disposed = true;
            _cleanupTimer?.Dispose();
            foreach (var pair in new List<KeyValuePair<string, Entry>>(_entries)) {
                DetachEntryLocked(pair.Key, pair.Value);
            }
        }
    }

    private void Release(string key, Entry entry) {
        lock (_gate) {
            if (entry.LeaseCount > 0)
                entry.LeaseCount--;

            if (entry.LeaseCount == 0 && (_disposed || entry.IsDetached || IsExpired(entry)))
                DisposeDetachedEntryIfUnused(entry);

            if (!_disposed) {
                EnqueueForEvictionIfEligible(key, entry);
                EnforceMaxItemsLocked();
            }
        }
    }

    private void EnforceMaxItemsLocked() {
        if (_options.MaxItems is not { } maxItems)
            return;

        while (_loadedItemCount > maxItems && _evictionQueue.TryDequeue(out var candidate, out var sequence)) {
            var entry = candidate.Entry;
            if (entry.IsDetached || entry.Image is null ||
                entry.LastAccessSequence != sequence || entry.LeaseCount != 0 || entry.WaiterCount != 0)
                continue;

            DetachEntryLocked(candidate.Key, entry);
        }
    }

    private void EnqueueForEvictionIfEligible(string key, Entry entry) {
        if (!entry.IsDetached && entry.Image is not null &&
            entry.LeaseCount == 0 && entry.WaiterCount == 0)
            _evictionQueue.Enqueue((key, entry), entry.LastAccessSequence);
    }

    private void CleanupExpiredEntries(object? state) {
        lock (_gate) {
            if (_disposed)
                return;

            foreach (var pair in _entries) {
                if (pair.Value.LeaseCount == 0 && pair.Value.LoadingTask is null && IsExpired(pair.Value)) {
                    _entries.Remove(pair.Key);
                    pair.Value.IsDetached = true;
                    DisposeDetachedEntryIfUnused(pair.Value);
                }
            }
        }
    }

    private void Touch(string key, Entry entry) {
        var now = _timeProvider.GetUtcNow();
        if (_options.SlidingExpiration is not null && !IsAbsoluteExpired(entry, now))
            entry.LastAccess = now;
        entry.LastAccessSequence = ++_accessSequence;
        EnqueueForEvictionIfEligible(key, entry);
    }

    private bool IsExpired(Entry entry) {
        var now = _timeProvider.GetUtcNow();
        return IsAbsoluteExpired(entry, now) ||
               _options.SlidingExpiration is { } sliding && now - entry.LastAccess >= sliding;
    }

    private bool IsAbsoluteExpired(Entry entry, DateTimeOffset now) {
        return _options.AbsoluteExpiration is { } absolute && now - entry.StrongSince >= absolute;
    }

    private TimeSpan GetCleanupPeriod() {
        var period = _options.AbsoluteExpiration ?? _options.SlidingExpiration ?? TimeSpan.FromMinutes(1);
        period = TimeSpan.FromTicks(Math.Max(period.Ticks / 2, TimeSpan.FromMilliseconds(100).Ticks));
        return period > TimeSpan.FromMinutes(1) ? TimeSpan.FromMinutes(1) : period;
    }

    private void DetachEntryLocked(string key, Entry entry) {
        if (_entries.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
            _entries.Remove(key);

        if (!entry.IsDetached) {
            entry.IsDetached = true;
            if (entry.Image is not null)
                _loadedItemCount--;
        }
        DisposeDetachedEntryIfUnused(entry);
    }

    private static void DisposeDetachedEntryIfUnused(Entry entry) {
        if (!entry.IsDetached || entry.LeaseCount != 0 || entry.WaiterCount != 0 || entry.LoadingTask is not null)
            return;

        DisposeImage(entry.Image);
        entry.Image = null;
    }

    private static void DisposeImage(IImage? image) {
        if(image is IDisposable disposable)
            disposable.Dispose();
    }

    private void ThrowIfDisposed() {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MemoryImageCache));
    }

    private sealed class Entry {
        public IImage? Image;
        public Task<IImage?>? LoadingTask;
        public TaskCompletionSource<IImage?>? Completion;
        public int WaiterCount;
        public int LeaseCount;
        public DateTimeOffset StrongSince;
        public DateTimeOffset LastAccess;
        public long LastAccessSequence;
        public bool IsDetached;
    }
}