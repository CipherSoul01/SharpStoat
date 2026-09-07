using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces;

namespace Stoat.Core.Image;

public static class StoatLoaders
{
    public static IAsyncImageLoader AsyncImageLoader { get; set; } =
        ImageLoaderPipelineBuilder.RamCached().Build();
}