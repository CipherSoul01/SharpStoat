using Stoat.Core.Interfaces.Leases;

namespace Stoat.Core.Interfaces.Caching;

public interface IImageCache
{
    Task<IImageLease?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(string key, IImageLease image, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    void Clear();
}