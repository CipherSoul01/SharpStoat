using System.Diagnostics;
using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services.Storage.OS.Linux;

[DebuggerDisplay("{DebuggerDisplay}")]
public class SecretServiceCredential : IStoatCredential 
{
    internal SecretServiceCredential(string service, string key, string password)
    {
        Service = service;
        Key = key;
        Password = password;
    }

    public string Service { get; }

    public string Key { get; }

    public string Password { get; }

    private string DebuggerDisplay => $"[Service: {Service}, Account: {Key}]";
}