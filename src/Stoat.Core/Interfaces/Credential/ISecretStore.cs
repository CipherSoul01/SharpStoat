namespace Stoat.Core.Interfaces.Credential;

public interface ISecretStore
{
    ValueTask<IStoatCredential?> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    ValueTask SetAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default);

    ValueTask<bool> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default);
}