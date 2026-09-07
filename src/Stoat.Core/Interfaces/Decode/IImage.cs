using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;
using Avalonia.Svg;
using Stoat.Core.Image.Primitives;

namespace Stoat.Core.Interfaces.Decode;

public interface IImage
{
    public object? Image { get; set; }
    public ImageFormat Format { get; }
}

public interface IImage<T> : IImage
    where T : class
{
    new T? Image { get; set; }
}

public interface IImageSvg : IImage<string>;
public interface IImageGif : IImage<IGifSource>;
public interface IImageBitmap : IImage<Bitmap>;