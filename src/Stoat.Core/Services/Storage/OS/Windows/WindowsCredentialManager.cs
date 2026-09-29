using System.Runtime.InteropServices;
using System.Text;
using Stoat.Core.Extensions;
using Stoat.Core.Interfaces.Credential;
using Stoat.Core.Interop.Windows.Native;

namespace Stoat.Core.Services.Storage.OS.Windows;

public class WindowsCredentialManager : ISecretStore 
{
    internal const string TargetNameLegacyGenericPrefix = "LegacyGeneric:target=";

    private readonly string _namespace;
    private readonly string _service; 

    /// <summary>
    /// Open the Windows Credential Manager vault for the current user.
    /// </summary>
    /// <param name="namespace">Optional namespace to scope credential operations.</param>
    /// <param name="service"></param>
    /// <returns>Current user's Credential Manager vault.</returns>
    public WindowsCredentialManager(string @namespace = "", string @service = "")
    {
        _namespace = @namespace;
        _service = @service;
    }

    public IList<string> GetAccounts(string service)
    {
        return Enumerate(service, null).Select(x => x.UserName).Distinct().ToList();
    }


    /// <summary>
    /// Check if we can persist credentials to for the current process and logon session.
    /// </summary>
    /// <returns>True if persistence is possible, false otherwise.</returns>
    public static bool CanPersist()
    {
        uint count = Advapi32.CRED_TYPE_MAXIMUM;
        var arr = new CredentialPersist[count];

        int result = Win32Error.GetLastError(
            Advapi32.CredGetSessionTypes(count, arr)
        );

        CredentialPersist persist = CredentialPersist.None;
        if (result == Win32Error.Success)
        {
            persist = arr[(int)CredentialType.Generic];
        }

        // If the maximum allowed is anything less than "local machine" then cannot persist credentials.
        return persist >= CredentialPersist.LocalMachine;
    }

    private IEnumerable<WindowsCredential> Enumerate(string service, string key)
    {
        IntPtr credList = IntPtr.Zero;

        try
        {
            int result = Win32Error.GetLastError(
                Advapi32.CredEnumerate(
                    null,
                    CredentialEnumerateFlags.AllCredentials,
                    out int count,
                    out credList)
            );

            switch (result)
            {
                case Win32Error.Success:
                    int ptrSize = Marshal.SizeOf<IntPtr>();
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr credPtr = Marshal.ReadIntPtr(credList, i * ptrSize);
                        Win32Credential credential = Marshal.PtrToStructure<Win32Credential>(credPtr);

                        if (!IsMatch(service, key, credential))
                        {
                            continue;
                        }

                        yield return CreateCredentialFromStructure(credential);
                    }
                    break;

                case Win32Error.NotFound:
                    yield break;

                default:
                    Win32Error.ThrowIfError(result, "Failed to enumerate credentials.");
                    yield break;
            }
        }
        finally
        {
            if (credList != IntPtr.Zero)
            {
                Advapi32.CredFree(credList);
            }
        }
    }

    private WindowsCredential CreateCredentialFromStructure(Win32Credential credential)
    {
        string password = credential.GetCredentialBlobAsString();

        // Recover the target name we gave from the internal (raw) target name
        string targetName = credential.TargetName.TrimUntilIndexOf(TargetNameLegacyGenericPrefix);

        // Recover the service name from the target name
        string serviceName = targetName;
        if (!string.IsNullOrWhiteSpace(_namespace))
        {
            serviceName = serviceName.TrimUntilIndexOf($"{_namespace}:");
        }

        // Strip any userinfo component from the service name
        serviceName = RemoveUriUserInfo(serviceName);

        return new WindowsCredential(serviceName, credential.UserName, password, targetName);
    }

    public /* for testing */ static string RemoveUriUserInfo(string url)
    {
        // To remove the userinfo component we must search for the end of the :// scheme
        // delimiter, and the start of the @ userinfo delimiter. We don't want to match
        // any other '@' character however (such as one in the URI path).
        // To ensure this we only consider an '@' character that exists before the first
        // '/' character after the scheme delimiter - that is to say the authority-path
        // separator.
        //
        //                authority
        //              |-----------|
        //     scheme://userinfo@host/path
        //
        int schemeDelimIdx = url.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
        if (schemeDelimIdx > 0)
        {
            int authorityIdx = schemeDelimIdx + Uri.SchemeDelimiter.Length;
            int slashIdx = url.IndexOf("/", authorityIdx, StringComparison.Ordinal);
            int atIdx = url.IndexOf("@", StringComparison.Ordinal);

            // No path component or trailing slash; use end of string
            if (slashIdx < 0)
            {
                slashIdx = url.Length - 1;
            }

            // Only if the '@' is before the first slash is this the userinfo delimiter
            if (0 < atIdx && atIdx < slashIdx)
            {
                return url.Substring(0, authorityIdx) + url.Substring(atIdx + 1);
            }
        }

        return url;
    }

    internal /* for testing */ bool IsMatch(string service, string account, Win32Credential credential)
    {
        // Match against the username first
        if (!string.IsNullOrWhiteSpace(account) &&
            !StringComparer.Ordinal.Equals(account, credential.UserName))
        {
            return false;
        }

        // Trim the "LegacyGeneric" prefix Windows adds
        string targetName = credential.TargetName.TrimUntilIndexOf(TargetNameLegacyGenericPrefix);
            
        // Only match credentials with the namespace we have been configured with (if any)
        if (!string.IsNullOrWhiteSpace(_namespace))
        {
            string nsPrefix = $"{_namespace}:";
            if (!targetName.StartsWith(nsPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            targetName = targetName.Substring(nsPrefix.Length);
        }

        // If the target name matches the service name exactly then return 'match'
        if (StringComparer.Ordinal.Equals(service, targetName))
        {
            return true;
        }

        // Try matching the target and service as URIs
        if (Uri.TryCreate(service, UriKind.Absolute, out Uri serviceUri) &&
            Uri.TryCreate(targetName, UriKind.Absolute, out Uri targetUri))
        {
            // Match scheme/protocol
            if (!StringComparer.OrdinalIgnoreCase.Equals(serviceUri.Scheme, targetUri.Scheme))
            {
                return false;
            }

            // Match host name
            if (!StringComparer.OrdinalIgnoreCase.Equals(serviceUri.Host, targetUri.Host))
            {
                return false;
            }

            // Match port number
            if (serviceUri.Port != targetUri.Port)
            {
                return false;
            }

            // Match path
            if (!string.IsNullOrWhiteSpace(serviceUri.AbsolutePath) &&
                !StringComparer.OrdinalIgnoreCase.Equals(serviceUri.AbsolutePath, targetUri.AbsolutePath))
            {
                return false;
            }

            // URLs match
            return true;
        }

        // Unable to match
        return false;
    }

    internal /* for testing */ string CreateTargetName(string service, string account)
    {
        var serviceUri = new Uri(service, UriKind.Absolute);
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_namespace))
        {
            sb.AppendFormat("{0}:", _namespace);
        }

        if (!string.IsNullOrWhiteSpace(serviceUri.Scheme))
        {
            sb.AppendFormat("{0}://", serviceUri.Scheme);
        }

        if (!string.IsNullOrWhiteSpace(account))
        {
            string escapedAccount = account.Replace('@', '_');
            sb.AppendFormat("{0}@", escapedAccount);
        }

        if (!string.IsNullOrWhiteSpace(serviceUri.Host))
        {
            sb.Append(serviceUri.Host);
        }

        if (!serviceUri.IsDefaultPort)
        {
            sb.AppendFormat(":{0}", serviceUri.Port);
        }

        string trimmedPath = serviceUri.AbsolutePath.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(trimmedPath))
        {
            sb.Append(trimmedPath);
        }

        return sb.ToString();
    }

    
    public ValueTask<IStoatCredential?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return new(Enumerate(_service, key).FirstOrDefault());
    }

    public ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_service))
        {
            throw new ArgumentException(
                "Service cannot be null or whitespace.",
                nameof(_service));
        }

        IntPtr existingCredPtr = IntPtr.Zero;
        IntPtr credBlob = IntPtr.Zero;

        try
        {
            string targetName = CreateTargetName(_service, account: null);
            if (Advapi32.CredRead(targetName, CredentialType.Generic, 0, out existingCredPtr))
            {
                var existingCred = Marshal.PtrToStructure<Win32Credential>(existingCredPtr);
                if (!StringComparer.Ordinal.Equals(existingCred.UserName, key))
                {
                    targetName = CreateTargetName(_service, key);
                }
                else
                {
                    string existingSecret = existingCred.GetCredentialBlobAsString();
                    if (StringComparer.Ordinal.Equals(existingSecret, value))
                    {
                        return new(Task.CompletedTask);
                    }
                }
            }

            byte[] secretBytes = Encoding.Unicode.GetBytes(value);
            credBlob = Marshal.AllocHGlobal(secretBytes.Length);
            Marshal.Copy(secretBytes, 0, credBlob, secretBytes.Length);

            var newCred = new Win32Credential
            {
                Type = CredentialType.Generic,
                TargetName = targetName,
                CredentialBlobSize = secretBytes.Length,
                CredentialBlob = credBlob,
                Persist = CredentialPersist.LocalMachine,
                UserName = key,
            };

            int result = Win32Error.GetLastError(
                Advapi32.CredWrite(ref newCred, 0)
            );

            Win32Error.ThrowIfError(result, "Failed to write item to store.");
        }
        finally
        {
            if (credBlob != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(credBlob);
            }

            if (existingCredPtr != IntPtr.Zero)
            {
                Advapi32.CredFree(existingCredPtr);
            }
        }
        
        return new (Task.CompletedTask);
    }

    public ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        WindowsCredential? credential = Enumerate(_service, key).FirstOrDefault();

        if (credential != null)
        {
            int result = Win32Error.GetLastError(
                Advapi32.CredDelete(credential.TargetName, CredentialType.Generic, 0)
            );

            switch (result)
            {
                case Win32Error.Success:
                    return new(true);

                case Win32Error.NotFound:
                    return new(false);

                default:
                    Win32Error.ThrowIfError(result);
                    return new(false);
            }
        }

        return new(false);
    }
}