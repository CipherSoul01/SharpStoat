using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces.Sources;

namespace Stoat.Core.Image.Sources;

public sealed class CompositeImageSourceResolver : IImageSourceResolver {
    private readonly IReadOnlyList<IImageSourceResolver> _resolvers;

    public CompositeImageSourceResolver(params IImageSourceResolver[] resolvers) {
        _resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
    }

    public async Task<ResolvedImageSource?> ResolveAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default) {
        foreach (var resolver in _resolvers) {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await resolver.ResolveAsync(request, cancellationToken).ConfigureAwait(false);
            if (result is not null)
                return result;
        }

        return null;
    }
}