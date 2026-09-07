using Stoat.Core.Interfaces.Decode;

namespace Stoat.Core.Interfaces.Leases;

public interface IImageLease : IDisposable
{
    IImage Image { get; }
}