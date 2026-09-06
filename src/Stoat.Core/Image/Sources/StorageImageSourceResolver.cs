using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces.Sources;

namespace Stoat.Core.Image.Sources;

public sealed class StorageImageSourceResolver : IImageSourceResolver {
    public async Task<ResolvedImageSource?> ResolveAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.StorageProvider is null ||
            !Uri.TryCreate(request.Source, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeFile && uri.Scheme != "content"))
            return null;

        try {
            var file = await request.StorageProvider.TryGetFileFromPathAsync(uri).ConfigureAwait(false);
            if (file is null)
                return null;

            cancellationToken.ThrowIfCancellationRequested();
            var stream = await file.OpenReadAsync().ConfigureAwait(false);
            return new ResolvedImageSource(stream);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        }
        catch (Exception) {
            return null;
        }
    }
}