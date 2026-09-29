using System.Diagnostics;
using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services.Storage.OS.MacOS;

[DebuggerDisplay("{DebuggerDisplay}")]
public class MacOSKeychainCredential : IStoatCredential 
{
    internal MacOSKeychainCredential(string service, string key, string password, string label)
    {
        Service = service;
        Key = key;
        Password = password;
        Label = label;
    }

    public string Service  { get; }

    public string Key { get; }

    public string Label { get; }

    public string Password { get; }

    private string DebuggerDisplay => $"{Label} [Service: {Service}, Account: {Key}]";
}