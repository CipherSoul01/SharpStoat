using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces.Leases;

namespace Stoat.Core.Interfaces;

public interface IAsyncImageLoader
{
    Task<IImageLease?> LoadAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default);
}