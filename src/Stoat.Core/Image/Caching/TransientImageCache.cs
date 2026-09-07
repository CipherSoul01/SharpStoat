using Stoat.Core.Image.Leases;
using Stoat.Core.Interfaces.Caching;
using Stoat.Core.Interfaces.Decode;
using Stoat.Core.Interfaces.Leases;

namespace Stoat.Core.Image.Caching;

public sealed class TransientImageCache : IImageMemoryCache {
    private bool _disposed;

    public async Task<IImageLease?> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<IImage?>> factory,
        CancellationToken cancellationToken = default) {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be empty.", nameof(key));
        ArgumentNullException.ThrowIfNull(factory);
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransientImageCache));

        var image = await factory(cancellationToken).ConfigureAwait(false);
        return image is null ? null : ImageLease.Owned(image);
    }

    public void Clear() {
    }

    public void Dispose() {
        _disposed = true;
    }
}