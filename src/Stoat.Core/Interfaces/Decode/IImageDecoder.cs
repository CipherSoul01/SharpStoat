namespace Stoat.Core.Interfaces.Decode;

public interface IImageDecoder
{
    Task<IImage> DecodeAsync(Stream stream, CancellationToken cancellationToken = default);
}