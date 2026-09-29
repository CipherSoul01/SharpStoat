using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Stoat.Core.Interfaces.Credential;
using Stoat.Core.Interop;
using Stoat.Core.Interop.MacOS.Native;
using static Stoat.Core.Interop.MacOS.Native.CoreFoundation;
using static Stoat.Core.Interop.MacOS.Native.SecurityFramework;

namespace Stoat.Core.Services.Storage.OS.MacOS;

public class MacOSKeychain : ISecretStore
{
    private readonly string _namespace;
    private readonly string _service;
    
    /// <summary>
    /// Open the default keychain (current user's login keychain).
    /// </summary>
    /// <param name="namespace">Optional namespace to scope credential operations.</param>
    /// <param name="service"></param>
    /// <returns>Default keychain.</returns>
    public MacOSKeychain(string @namespace = "", string @service = "")
    {
        _namespace = @namespace;
        _service = @service;
    }
    
    public IList<string> GetAccounts(string service)
    {
        IntPtr query = IntPtr.Zero;
        IntPtr resultPtr = IntPtr.Zero;
        IntPtr servicePtr = IntPtr.Zero;
        IntPtr accountPtr = IntPtr.Zero;

        try
        {
            query = CFDictionaryCreateMutable(
                IntPtr.Zero,
                0,
                IntPtr.Zero, IntPtr.Zero);

            CFDictionaryAddValue(query, kSecClass, kSecClassGenericPassword);
            CFDictionaryAddValue(query, kSecMatchLimit, kSecMatchLimitAll);
            CFDictionaryAddValue(query, kSecReturnAttributes, kCFBooleanTrue);

            if (!string.IsNullOrWhiteSpace(service))
            {
                string fullService = CreateServiceName(service);
                servicePtr = CreateCFStringUtf8(fullService);
                CFDictionaryAddValue(query, kSecAttrService, servicePtr);
            }

            int searchResult = SecItemCopyMatching(query, out resultPtr);

            switch (searchResult)
            {
                case OK:
                    int typeId = CFGetTypeID(resultPtr);
                    Debug.Assert(typeId == CFArrayGetTypeID(), "Returned unknown item from account query");
                    if (typeId == CFArrayGetTypeID())
                    {
                        int len = (int)CFArrayGetCount(resultPtr);
                        var accounts = new HashSet<string>();
                        for (int i = 0; i < len; i++)
                        {
                            IntPtr dict = CFArrayGetValueAtIndex(resultPtr, i);
                            string account = GetStringAttribute(dict, kSecAttrAccount);
                            accounts.Add(account);
                        }

                        return accounts.ToList();
                    }

                    throw new InteropException($"Unknown keychain search result type CFTypeID: {typeId}.", -1);

                case ErrorSecItemNotFound:
                    return Array.Empty<string>();

                default:
                    ThrowIfError(searchResult);
                    return null;
            }
        }
        finally
        {
            if (query != IntPtr.Zero) CFRelease(query);
            if (servicePtr != IntPtr.Zero) CFRelease(servicePtr);
            if (accountPtr != IntPtr.Zero) CFRelease(accountPtr);
            if (resultPtr != IntPtr.Zero) CFRelease(resultPtr);
        }
    }

    private static IntPtr CreateCFStringUtf8(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return CFStringCreateWithBytes(IntPtr.Zero,
            bytes, bytes.Length, CFStringEncoding.kCFStringEncodingUTF8, false);
    }

    private static IStoatCredential CreateCredentialFromAttributes(IntPtr attributes)
    {
        string service = GetStringAttribute(attributes, kSecAttrService);
        string account = GetStringAttribute(attributes, kSecAttrAccount);
        string password = GetStringAttribute(attributes, kSecValueData);
        string label = GetStringAttribute(attributes, kSecAttrLabel);
        return new MacOSKeychainCredential(service, account, password, label);
    }

    private static string GetStringAttribute(IntPtr dict, IntPtr key)
    {
        if (dict == IntPtr.Zero)
        {
            return null;
        }

        if (CFDictionaryGetValueIfPresent(dict, key, out IntPtr value) && value != IntPtr.Zero)
        {
            if (CFGetTypeID(value) == CFStringGetTypeID())
            {
                return CFStringToString(value);
            }

            if (CFGetTypeID(value) == CFDataGetTypeID())
            {
                int length = CFDataGetLength(value);
                IntPtr ptr = CFDataGetBytePtr(value);
                return Marshal.PtrToStringAuto(ptr, length);
            }
        }

        return null;
    }

    private string CreateServiceName(string service)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(_namespace))
        {
            sb.AppendFormat("{0}:", _namespace);
        }

        sb.Append(service);
        return sb.ToString();
    }

    public ValueTask<IStoatCredential?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        IntPtr query = IntPtr.Zero;
        IntPtr resultPtr = IntPtr.Zero;
        IntPtr servicePtr = IntPtr.Zero;
        IntPtr accountPtr = IntPtr.Zero;

        try
        {
            query = CFDictionaryCreateMutable(
                IntPtr.Zero,
                0,
                IntPtr.Zero, IntPtr.Zero);

            CFDictionaryAddValue(query, kSecClass, kSecClassGenericPassword);
            CFDictionaryAddValue(query, kSecMatchLimit, kSecMatchLimitOne);
            CFDictionaryAddValue(query, kSecReturnData, kCFBooleanTrue);
            CFDictionaryAddValue(query, kSecReturnAttributes, kCFBooleanTrue);

            if (!string.IsNullOrWhiteSpace(_service))
            {
                string fullService = CreateServiceName(_service);
                servicePtr = CreateCFStringUtf8(fullService);
                CFDictionaryAddValue(query, kSecAttrService, servicePtr);
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                accountPtr = CreateCFStringUtf8(key);
                CFDictionaryAddValue(query, kSecAttrAccount, accountPtr);
            }

            int searchResult = SecItemCopyMatching(query, out resultPtr);

            switch (searchResult)
            {
                case OK:
                    int typeId = CFGetTypeID(resultPtr);
                    Debug.Assert(typeId != CFArrayGetTypeID(), "Returned more than one keychain item in search");
                    if (typeId == CFDictionaryGetTypeID())
                    {
                        return new (CreateCredentialFromAttributes(resultPtr));
                    }

                    throw new InteropException($"Unknown keychain search result type CFTypeID: {typeId}.", -1);

                case ErrorSecItemNotFound:
                    return new();

                default:
                    ThrowIfError(searchResult);
                    return new();
            }
        }
        finally
        {
            if (query != IntPtr.Zero) CFRelease(query);
            if (servicePtr != IntPtr.Zero) CFRelease(servicePtr);
            if (accountPtr != IntPtr.Zero) CFRelease(accountPtr);
            if (resultPtr != IntPtr.Zero) CFRelease(resultPtr);
        } 
    }

    public ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_service))
        {
            throw new ArgumentException(
                "Service cannot be null or whitespace.",
                nameof(_service));
        }

        byte[] secretBytes = Encoding.UTF8.GetBytes(value);

        IntPtr passwordData = IntPtr.Zero;
        IntPtr itemRef = IntPtr.Zero;

        string serviceName = CreateServiceName(_service);

        uint serviceNameLength = (uint) serviceName.Length;
        uint accountLength = (uint) (key?.Length ?? 0);

        try
        {
            // Check if an entry already exists in the keychain
            int findResult = SecKeychainFindGenericPassword(
                IntPtr.Zero, serviceNameLength, serviceName, accountLength, key,
                out uint passwordDataLength, out passwordData, out itemRef);

            switch (findResult)
            {
                // Update existing entry only if the password/secret is different
                case OK when !InteropUtils.AreEqual(secretBytes, passwordData, passwordDataLength):
                    ThrowIfError(
                        SecKeychainItemModifyAttributesAndData(itemRef, IntPtr.Zero, (uint) secretBytes.Length, secretBytes),
                        "Could not update existing item"
                    );
                    break;

                // Create new entry
                case ErrorSecItemNotFound:
                    ThrowIfError(
                        SecKeychainAddGenericPassword(IntPtr.Zero, serviceNameLength, serviceName, accountLength,
                            key, (uint) secretBytes.Length, secretBytes, out itemRef),
                        "Could not create new item"
                    );
                    break;

                default:
                    ThrowIfError(findResult);
                    break;
            }
        }
        finally
        {
            if (passwordData != IntPtr.Zero)
            {
                SecKeychainItemFreeContent(IntPtr.Zero, passwordData);
            }

            if (itemRef != IntPtr.Zero)
            {
                CFRelease(itemRef);
            }
        }

        return new();
    }

    public ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        IntPtr query = IntPtr.Zero;
        IntPtr itemRefPtr = IntPtr.Zero;
        IntPtr servicePtr = IntPtr.Zero;
        IntPtr accountPtr = IntPtr.Zero;

        try
        {
            query = CFDictionaryCreateMutable(
                IntPtr.Zero,
                0,
                IntPtr.Zero, IntPtr.Zero);

            CFDictionaryAddValue(query, kSecClass, kSecClassGenericPassword);
            CFDictionaryAddValue(query, kSecMatchLimit, kSecMatchLimitOne);
            CFDictionaryAddValue(query, kSecReturnRef, kCFBooleanTrue);

            if (!string.IsNullOrWhiteSpace(_service))
            {
                string fullService = CreateServiceName(_service);
                servicePtr = CreateCFStringUtf8(fullService);
                CFDictionaryAddValue(query, kSecAttrService, servicePtr);
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                accountPtr = CreateCFStringUtf8(key);
                CFDictionaryAddValue(query, kSecAttrAccount, accountPtr);
            }

            int searchResult = SecItemCopyMatching(query, out itemRefPtr);
            switch (searchResult)
            {
                case OK:
                    ThrowIfError(
                        SecKeychainItemDelete(itemRefPtr)
                    );
                    return new(true);

                case ErrorSecItemNotFound:
                    return new(false);

                default:
                    ThrowIfError(searchResult);
                    return new(false);
            }
        }
        finally
        {
            if (query != IntPtr.Zero) CFRelease(query);
            if (itemRefPtr != IntPtr.Zero) CFRelease(itemRefPtr);
            if (servicePtr != IntPtr.Zero) CFRelease(servicePtr);
            if (accountPtr != IntPtr.Zero) CFRelease(accountPtr);
        }
    }
}