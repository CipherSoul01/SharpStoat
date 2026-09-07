using Stoat.Core.Image.Caching;
using Stoat.Core.Image.Decode;
using Stoat.Core.Image.Sources;
using Stoat.Core.Image.Transport;
using Stoat.Core.Interfaces.Caching;
using Stoat.Core.Interfaces.Decode;
using Stoat.Core.Interfaces.Sources;
using Stoat.Core.Interfaces.Transport;

namespace Stoat.Core.Image.Pipeline;

public sealed class ImageLoaderPipelineBuilder {
    private IImageSourceResolver? _sourceResolver;
    private IImageTransport? _transport;
    private IImageDecoder? _decoder;
    private IImageMemoryCache? _memoryCache;
    private IImageByteCache? _byteCache;
    private HttpClient? _httpClient;
    private bool _disposeHttpClient;
    private bool _built;

    private ImageLoaderPipelineBuilder(IImageMemoryCache memoryCache, IImageByteCache? byteCache = null) {
        _memoryCache = memoryCache;
        _byteCache = byteCache;
    }

    public static ImageLoaderPipelineBuilder Uncached() {
        return new ImageLoaderPipelineBuilder(new TransientImageCache());
    }

    public static ImageLoaderPipelineBuilder RamCached(MemoryImageCacheOptions? options = null) {
        return new ImageLoaderPipelineBuilder(new MemoryImageCache(options));
    }

    internal static ImageLoaderPipelineBuilder RamCached(
        MemoryImageCacheOptions? options,
        TimeProvider timeProvider) {
        return new ImageLoaderPipelineBuilder(new MemoryImageCache(options, timeProvider));
    }

    public static ImageLoaderPipelineBuilder DiskCached(
        string cacheFolder = "Cache/Images/",
        MemoryImageCacheOptions? memoryCacheOptions = null) {
        return new ImageLoaderPipelineBuilder(
            new MemoryImageCache(memoryCacheOptions),
            new DiskImageByteCache(cacheFolder));
    }

    public ImageLoaderPipelineBuilder UseSourceResolver(IImageSourceResolver sourceResolver) {
        EnsureNotBuilt();
        _sourceResolver = sourceResolver ?? throw new ArgumentNullException(nameof(sourceResolver));
        return this;
    }

    public ImageLoaderPipelineBuilder UseTransport(IImageTransport transport) {
        EnsureNotBuilt();
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        return this;
    }

    public ImageLoaderPipelineBuilder UseDecoder(IImageDecoder decoder) {
        EnsureNotBuilt();
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        return this;
    }

    public ImageLoaderPipelineBuilder UseMemoryCache(IImageMemoryCache memoryCache) {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(memoryCache);
        _memoryCache?.Dispose();
        _memoryCache = memoryCache;
        return this;
    }

    public ImageLoaderPipelineBuilder UseByteCache(IImageByteCache? byteCache) {
        EnsureNotBuilt();
        _byteCache = byteCache;
        return this;
    }

    public ImageLoaderPipelineBuilder UseHttpClient(HttpClient httpClient, bool disposeHttpClient = false) {
        EnsureNotBuilt();
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
        return this;
    }

    public ImageLoaderPipeline Build() {
        EnsureNotBuilt();
        _built = true;

        var ownedResources = new List<IDisposable>();
        var transport = _transport;
        if (transport is null) {
            var httpClient = _httpClient;
            if (httpClient is null) {
                httpClient = new HttpClient();
                ownedResources.Add(httpClient);
            }
            else if (_disposeHttpClient) {
                ownedResources.Add(httpClient);
            }

            transport = new HttpImageTransport(httpClient);
        }

        return new ImageLoaderPipeline(
            _sourceResolver ?? CreateDefaultSourceResolver(),
            transport,
            _decoder ?? new ImageDecode(),
            _memoryCache!,
            _byteCache,
            ownedResources);
    }

    private static IImageSourceResolver CreateDefaultSourceResolver() {
        return new CompositeImageSourceResolver(
            new FileImageSourceResolver(),
            new StorageImageSourceResolver(),
            new AvaloniaAssetSourceResolver());
    }

    private void EnsureNotBuilt() {
        if (_built)
            throw new InvalidOperationException("This builder has already built a pipeline.");
    }
}