using Stoat.Api.Interfaces;
using Stoat.Api.Model;

namespace Stoat.Api.Services;

public class StoatServerConfiguration : IStoatServerConfiguration
{
    public Uri? BaseAddress { get; private set; }
    public StoatConfig? Config { get; private set; }

    public void SetBaseAddress(Uri baseAddress)
        => BaseAddress = baseAddress;
    
    public void SetConfig(StoatConfig config)
        => Config = config;
}