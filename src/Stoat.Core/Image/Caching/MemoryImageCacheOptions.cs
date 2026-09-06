namespace Stoat.Core.Image.Caching;

public sealed class MemoryImageCacheOptions {
    
    public TimeSpan? AbsoluteExpiration { get; init; }

    public TimeSpan? SlidingExpiration { get; init; }

    public uint? MaxItems { get; init; }

    public void Validate() {
        if (AbsoluteExpiration is { } absolute && absolute <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(AbsoluteExpiration));

        if (SlidingExpiration is { } sliding && sliding <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(SlidingExpiration));

        if (MaxItems is 0)
            throw new ArgumentOutOfRangeException(nameof(MaxItems));
    }
}