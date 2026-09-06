using Stoat.Core.Interfaces.Decode;
using Stoat.Core.Interfaces.Leases;

namespace Stoat.Core.Interfaces.Caching;

public interface IImageMemoryCache : IDisposable
{
    Task<IImageLease?> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<IImage?>> factory,
        CancellationToken cancellationToken = default);

    void Clear();
}