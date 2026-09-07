using Avalonia.Platform.Storage;

namespace Stoat.Core.Image.Pipeline;

public sealed record ImageLoadRequest {
   
    public ImageLoadRequest(
        string source,
        Uri? baseUri = null,
        IStorageProvider? storageProvider = null) {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Image source cannot be empty.", nameof(source));

        Source = source;
        BaseUri = baseUri;
        StorageProvider = storageProvider;
    }

    public string Source { get; }

    public Uri? BaseUri { get; }

    public IStorageProvider? StorageProvider { get; }
}