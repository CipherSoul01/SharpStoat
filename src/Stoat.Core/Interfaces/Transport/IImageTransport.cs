using Stoat.Core.Image.Pipeline;

namespace Stoat.Core.Interfaces.Transport;

public interface IImageTransport
{
    Task<Stream?> GetAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default);
}