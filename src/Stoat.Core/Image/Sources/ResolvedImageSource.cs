namespace Stoat.Core.Image.Sources;

public sealed class ResolvedImageSource : IDisposable {
    private Stream? _stream;

    public ResolvedImageSource(Stream stream) {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public Stream Stream => _stream ?? throw new ObjectDisposedException(nameof(ResolvedImageSource));

    public void Dispose() {
        _stream?.Dispose();
        _stream = null;
    }
}