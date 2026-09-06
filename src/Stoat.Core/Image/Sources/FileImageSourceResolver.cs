using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces.Sources;

namespace Stoat.Core.Image.Sources;

public sealed class FileImageSourceResolver : IImageSourceResolver {
    public Task<ResolvedImageSource?> ResolveAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        var path = request.Source;
        if (Uri.TryCreate(request.Source, UriKind.Absolute, out var uri) && uri.IsFile)
            path = uri.LocalPath;

        if (!File.Exists(path))
            return Task.FromResult<ResolvedImageSource?>(null);

        return Task.FromResult<ResolvedImageSource?>(
            new ResolvedImageSource(File.OpenRead(path)));
    }
}