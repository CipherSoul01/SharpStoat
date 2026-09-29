using Stoat.Core.Interfaces.Credential;
using Stoat.Core.Services.Storage.OS.Linux;
using Stoat.Core.Services.Storage.OS.MacOS;
using Stoat.Core.Services.Storage.OS.Windows;

namespace Stoat.Core.Services.Storage;

public class OsStorage : IStoatOsStore
{
    private readonly ISecretStore _systemStore;
    private const string Service = "stoat.chat";
    private const string Namespace = "stoat";
    
    public OsStorage()
    {
        _systemStore = CreateSystemStore();
    }

    private static ISecretStore CreateSystemStore()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsCredentialManager();

        if (OperatingSystem.IsLinux())
            return new SecretServiceCollection(Namespace, Service);

        if (OperatingSystem.IsMacOS())
            return new MacOSKeychain(Namespace,  Service);
        
        throw new PlatformNotSupportedException(
            "No supported secure credential store is available on this platform.");
    }

    public async ValueTask<IStoatCredential?> GetAsync(string key, CancellationToken cancellationToken = default)
        => await _systemStore.GetAsync(key, cancellationToken);

    public async ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
        => await _systemStore.SetAsync(key,value, cancellationToken);

    public async ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        => await _systemStore.DeleteAsync(key, cancellationToken);
}