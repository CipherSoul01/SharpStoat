using Stoat.Core.Image.Pipeline;
using Stoat.Core.Image.Sources;

namespace Stoat.Core.Interfaces.Sources;

public interface IImageSourceResolver {

    Task<ResolvedImageSource?> ResolveAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default);
}