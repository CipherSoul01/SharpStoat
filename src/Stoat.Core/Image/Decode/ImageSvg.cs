using Avalonia.Svg;
using Stoat.Core.Image.Primitives;
using Stoat.Core.Interfaces.Decode;

namespace Stoat.Core.Image.Decode;

public class ImageSvg : IImageSvg
{
    object? IImage.Image
    {
        get => Image;
        set => Image = (string?)value;
    }

    public string? Image { get; set; }
    public ImageFormat Format => ImageFormat.Svg;

    public ImageSvg(string? svg)
        => Image = svg;
}