using Avalonia.Media.Imaging;
using Stoat.Core.Image.Primitives;
using Stoat.Core.Interfaces.Decode;

namespace Stoat.Core.Image.Decode;

public class ImageBitmap : IImageBitmap
{
    object? IImage.Image
    {
        get => Image;
        set => Image = (Bitmap)value;
    }

    public Bitmap? Image { get; set; }
    public ImageFormat Format => ImageFormat.Bitmap;
    
    public ImageBitmap(Bitmap image)
        =>  Image = image;
}