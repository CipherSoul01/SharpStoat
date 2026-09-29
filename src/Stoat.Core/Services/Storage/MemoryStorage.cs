using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services.Storage;

public class MemoryStorage : IStoatMemoryStore
{
    private readonly IStoatOsStore _osStore;

    public MemoryStorage(IStoatOsStore osStore)
    {
        _osStore = osStore;
    }

    public ValueTask<IStoatCredential?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool IsInitialized { get; }
    
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}