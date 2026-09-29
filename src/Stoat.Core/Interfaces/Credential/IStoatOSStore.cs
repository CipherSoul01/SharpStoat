namespace Stoat.Core.Interfaces.Credential;

public interface IStoatMemoryStore : ISecretStore 
{
    bool IsInitialized { get; }
    
    Task InitializeAsync(CancellationToken cancellationToken = default);    
}

public interface IStoatOsStore : ISecretStore;