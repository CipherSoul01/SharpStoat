using Avalonia.Labs.Gif;
using Stoat.Core.Image.Primitives;
using Stoat.Core.Interfaces.Decode;

namespace Stoat.Core.Image.Decode;

public class ImageGif : IImageGif
{
    object? IImage.Image
    {
        get => Image;
        set => Image = (IGifSource)value;
    }

    public IGifSource? Image { get; set; }
    public ImageFormat Format => ImageFormat.Gif;
    
    public ImageGif(IGifSource image)
        => Image = image;
}