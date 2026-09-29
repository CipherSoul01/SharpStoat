using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services.Storage;

public sealed class LocalStorage : ISecretStore
{
    private readonly IStoatMemoryStore _memoryStore;
    private readonly IStoatOsStore _osStore;
    
    private const string _domain = "stoat";

    public LocalStorage(IStoatMemoryStore memoryStore, IStoatOsStore osStore)
    {
        _memoryStore = memoryStore;
        _osStore = osStore;
    }

    private async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        if(!_memoryStore.IsInitialized)
            await _memoryStore.InitializeAsync(cancellationToken);
    }

    private bool IsOsStored(string domain, string key)
        => domain.Equals("stoat", StringComparison.OrdinalIgnoreCase) &&
           (key.Equals("P0", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("P2", StringComparison.OrdinalIgnoreCase));

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }

    public async ValueTask<IStoatCredential?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        await BootstrapAsync(cancellationToken);
        
        cancellationToken.ThrowIfCancellationRequested(); 
        
        if (IsOsStored(_domain, key))
        {
            var result = await _osStore.GetAsync(key, cancellationToken);

            if (result == null)
            {
                result = await _osStore.GetAsync(key, cancellationToken);
            }
            
            return result;
        }
        
        return await _memoryStore.GetAsync(key, cancellationToken);
    }

    public async ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await BootstrapAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        
        if (IsOsStored(_domain, key))
        {
            await _osStore.SetAsync(key, value, cancellationToken);
            
            return;
        }
        
        await _memoryStore.SetAsync(key, value, cancellationToken);
    }

    public async ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await BootstrapAsync(cancellationToken);
        
        cancellationToken.ThrowIfCancellationRequested();
        
        if(IsOsStored(_domain, key))
            return await _osStore.DeleteAsync(key, cancellationToken);
        
        return await _memoryStore.DeleteAsync(key, cancellationToken);
    }
}