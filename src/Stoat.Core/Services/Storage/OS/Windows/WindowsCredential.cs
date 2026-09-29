
using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services.Storage.OS.Windows;

public class WindowsCredential : IStoatCredential
{
    public WindowsCredential(string service, string userName, string password, string targetName)
    {
        Service = service;
        UserName = userName;
        Password = password;
        TargetName = targetName;
    }

    public string Service { get; }

    public string UserName { get; }

    public string Password { get; }

    public string TargetName { get; }

    string IStoatCredential.Key => UserName;
}