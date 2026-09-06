namespace Stoat.Core.Interfaces.Caching;

public interface IImageByteCache
{
    Task<Stream?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(string key, Stream data, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    void Clear();
}