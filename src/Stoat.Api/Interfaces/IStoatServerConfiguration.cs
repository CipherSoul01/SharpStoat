using Stoat.Api.Model;

namespace Stoat.Api.Interfaces;

public interface IStoatServerConfiguration
{
    Uri? BaseAddress { get; }
    
    StoatConfig? Config { get; }

    public void SetBaseAddress(Uri baseAddress);

    public void SetConfig(StoatConfig config);
}