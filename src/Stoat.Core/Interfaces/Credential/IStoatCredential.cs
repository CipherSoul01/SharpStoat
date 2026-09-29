namespace Stoat.Core.Interfaces.Credential;

public interface IStoatCredential
{
    string Key { get; }
    string Password { get; }
}