using System.Text;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;
using Avalonia.Svg;
using Stoat.Core.Image.Primitives;
using Stoat.Core.Interfaces.Decode;

namespace Stoat.Core.Image.Decode;

public class ImageDecode : IImageDecoder
{
    public async Task<IImage> DecodeAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        if (!stream.CanSeek)
        {
            var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream, cancellationToken);
            await stream.DisposeAsync();

            memoryStream.Position = 0;
            stream = memoryStream;
        }

        var buffer = new byte[512];

        var read = await stream.ReadAsync(
            buffer.AsMemory(),
            cancellationToken);

        stream.Position = 0;

        var type = DetectType(buffer.AsSpan(0, read));

        return type switch
        {
            ImageFormat.Svg => new ImageSvg(await ReadSvgAsync(stream, cancellationToken)),
            ImageFormat.Gif => new ImageGif(GifStreamSource.FromStream(stream)),
            ImageFormat.Bitmap => new ImageBitmap(new Bitmap(stream)),
            _ => throw new NotSupportedException()
        };
    }

    private static ImageFormat DetectType(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 6 &&
            (header[..6].SequenceEqual("GIF87a"u8) ||
             header[..6].SequenceEqual("GIF89a"u8)))
        {
            return ImageFormat.Gif;
        }

        if (IsSvg(header))
            return ImageFormat.Svg;

        return ImageFormat.Bitmap;
    }

    private static async Task<string> ReadSvgAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        return await reader.ReadToEndAsync(cancellationToken);
    }
    
    private static bool IsSvg(ReadOnlySpan<byte> data)
    {
        var text = Encoding.UTF8.GetString(data);

        text = text.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');

        while (true)
        {
            if (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                var end = text.IndexOf("?>", StringComparison.Ordinal);

                if (end < 0)
                    break;

                text = text[(end + 2)..].TrimStart();
                continue;
            }

            if (text.StartsWith("<!--", StringComparison.Ordinal))
            {
                var end = text.IndexOf("-->", StringComparison.Ordinal);

                if (end < 0)
                    break;

                text = text[(end + 3)..].TrimStart();
                continue;
            }

            break;
        }

        return text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase);
    }
}
