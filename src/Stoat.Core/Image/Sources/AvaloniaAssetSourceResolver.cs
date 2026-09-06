using Avalonia.Platform;
using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces.Sources;

namespace Stoat.Core.Image.Sources;

public sealed class AvaloniaAssetSourceResolver : IImageSourceResolver {
    /// <inheritdoc />
    public Task<ResolvedImageSource?> ResolveAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Uri.TryCreate(request.Source, UriKind.RelativeOrAbsolute, out var uri) ||
            IsExternalUri(uri))
            return Task.FromResult<ResolvedImageSource?>(null);

        try {
            if (!AssetLoader.Exists(uri, request.BaseUri))
                return Task.FromResult<ResolvedImageSource?>(null);

            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<ResolvedImageSource?>(
                new ResolvedImageSource(AssetLoader.Open(uri, request.BaseUri)));
        }
        catch (Exception) {
            return Task.FromResult<ResolvedImageSource?>(null);
        }
    }

    private static bool IsExternalUri(Uri uri) {
        return uri.IsAbsoluteUri &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}