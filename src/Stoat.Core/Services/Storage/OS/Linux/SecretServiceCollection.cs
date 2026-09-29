using System.Runtime.InteropServices;
using System.Text;
using Stoat.Core.Interfaces.Credential;
using Stoat.Core.Interop;
using static Stoat.Core.Interop.Linux.Native.Gobject;
using static Stoat.Core.Interop.Linux.Native.Glib;
using static Stoat.Core.Interop.Linux.Native.Libsecret;
using static Stoat.Core.Interop.Linux.Native.Libsecret.SecretSchemaAttributeType;
using static Stoat.Core.Interop.Linux.Native.Libsecret.SecretSchemaFlags;

namespace Stoat.Core.Services.Storage.OS.Linux;

public class SecretServiceCollection : ISecretStore 
{
    private const string SchemaName = "dev.stoat.CredentialManager";
    private const string ServiceAttributeName = "service";
    private const string AccountAttributeName = "account";
    private const string PlainTextContentType = "plain/text";

    private readonly string _namespace;
    private readonly string _service;
    
    public SecretServiceCollection(string @namespace, string @service)
    {
        _namespace = @namespace;
        _service = @service;
    }

    #region ICredentialStore

    public IList<string> GetAccounts(string service)
    {
        return Enumerate(service, null).Select(x => x.Key).Distinct().ToList();
    }

    private unsafe IEnumerable<IStoatCredential> Enumerate(string service, string key)
    {
        GHashTable* queryAttrs = null;
        GList* results = null;
        GError* error = null;

        try
        {
            SecretService* secService = GetSecretService();

            queryAttrs = CreateSearchQuery(service, key);

            SecretSchema schema = GetSchema();

            results = secret_service_search_sync(
                secService,
                ref schema,
                queryAttrs,
                SecretSearchFlags.SECRET_SEARCH_UNLOCK | SecretSearchFlags.SECRET_SEARCH_ALL,
                IntPtr.Zero,
                out error);

            if (error != null)
            {
                int code = error->code;
                string message = Marshal.PtrToStringAuto(error->message)!;
                throw new InteropException("Failed to search for credentials", code, new Exception(message));
            }

            var credentials = new List<IStoatCredential>();

            GList* itemPtr = results;
            while (itemPtr != null && itemPtr->data != IntPtr.Zero)
            {
                SecretItem* item = (SecretItem*) itemPtr->data;

                if (secret_item_get_locked(item))
                {
                    var toUnlockList = new GList
                    {
                        data = (IntPtr) item,
                        next = IntPtr.Zero,
                        prev = IntPtr.Zero
                    };

                    int numUnlocked = secret_service_unlock_sync(
                        secService,
                        &toUnlockList,
                        IntPtr.Zero,
                        out _,
                        out error
                    );

                    if (numUnlocked != 1)
                    {
                        throw new InteropException("Failed to unlock item", numUnlocked);
                    }
                }

                credentials.Add(CreateCredentialFromItem(item));

                itemPtr = (GList*)itemPtr->next;
            }

            return credentials;
        }
        finally
        {
            if (queryAttrs != null) g_hash_table_destroy(queryAttrs);
            if (error != null) g_error_free(error);
            if (results != null) g_list_free_full(results, g_object_unref);
        }
    }

    #endregion

    private unsafe GHashTable* CreateSearchQuery(string service, string key)
    {
        GHashTable* queryAttrs = g_hash_table_new_full(
            g_str_hash, g_str_equal,
            Marshal.FreeHGlobal, Marshal.FreeHGlobal);

        if (!string.IsNullOrWhiteSpace(service))
        {
            string fullServiceName = CreateServiceName(service);
            IntPtr keyPtr = Marshal.StringToHGlobalAnsi(ServiceAttributeName);
            IntPtr valuePtr = Marshal.StringToHGlobalAnsi(fullServiceName);
            g_hash_table_insert(queryAttrs, keyPtr, valuePtr);
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            IntPtr keyPtr = Marshal.StringToHGlobalAnsi(AccountAttributeName);
            IntPtr valuePtr = Marshal.StringToHGlobalAnsi(key);
            g_hash_table_insert(queryAttrs, keyPtr, valuePtr);
        }

        return queryAttrs;
    }

    private static unsafe IStoatCredential CreateCredentialFromItem(SecretItem* item)
    {
        GHashTable* secretAttrs = null;
        IntPtr serviceKeyPtr = IntPtr.Zero;
        IntPtr accountKeyPtr = IntPtr.Zero;
        SecretValue* value = null;
        IntPtr passwordPtr = IntPtr.Zero;
        GError* error = null;

        try
        {
            secretAttrs = secret_item_get_attributes(item);

            serviceKeyPtr = Marshal.StringToHGlobalAnsi(ServiceAttributeName);
            IntPtr serviceValuePtr = g_hash_table_lookup(secretAttrs, serviceKeyPtr);
            string service = Marshal.PtrToStringAuto(serviceValuePtr);

            accountKeyPtr = Marshal.StringToHGlobalAnsi(AccountAttributeName);
            IntPtr accountValuePtr = g_hash_table_lookup(secretAttrs, accountKeyPtr);
            string account = Marshal.PtrToStringAuto(accountValuePtr);

            secret_item_load_secret_sync(item, IntPtr.Zero, out error);
            value = secret_item_get_secret(item);
            if (value == null)
            {
                throw new InteropException("Failed to load secret", -1);
            }

            passwordPtr = secret_value_get(value, out int passwordLength);
            string password = Marshal.PtrToStringAuto(passwordPtr, passwordLength);

            return new SecretServiceCredential(service, account, password);
        }
        finally
        {
            if (secretAttrs != null) g_hash_table_unref(secretAttrs);
            if (accountKeyPtr != IntPtr.Zero) Marshal.FreeHGlobal(accountKeyPtr);
            if (serviceKeyPtr != IntPtr.Zero) Marshal.FreeHGlobal(serviceKeyPtr);
            if (value != null) secret_value_unref(value);
            if (error != null) g_error_free(error);
        }
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

    private static unsafe SecretService* GetSecretService()
    {
        SecretService* service = secret_service_get_sync(
            SecretServiceFlags.SECRET_SERVICE_OPEN_SESSION | SecretServiceFlags.SECRET_SERVICE_LOAD_COLLECTIONS,
            IntPtr.Zero, out GError* error);

        if (error != null)
        {
            int code = error->code;
            string message = Marshal.PtrToStringAuto(error->message)!;
            g_error_free(error);
            throw new InteropException("Failed to open secret service session", code, new Exception(message));
        }

        return service;
    }

    private static SecretSchema GetSchema()
    {
        var schema = new SecretSchema
        {
            name = SchemaName,
            flags = SECRET_SCHEMA_DONT_MATCH_NAME,
            attributes = new SecretSchemaAttribute[32]
        };

        schema.attributes[0] = new SecretSchemaAttribute
        {
            name = ServiceAttributeName,
            type = SECRET_SCHEMA_ATTRIBUTE_STRING
        };

        schema.attributes[1] = new SecretSchemaAttribute
        {
            name = AccountAttributeName,
            type = SECRET_SCHEMA_ATTRIBUTE_STRING
        };

        return schema;
    }

    public ValueTask<IStoatCredential?> GetAsync(string key, CancellationToken cancellationToken = default)
        => new(Task.FromResult(Enumerate(_service, key).FirstOrDefault()));

    public unsafe ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        GHashTable* attributes = null;
        SecretValue* secretValue = null;
        GError *error = null;

        IStoatCredential? existingCred = Enumerate(_service, key).FirstOrDefault();
        
        if (existingCred != null &&
            StringComparer.Ordinal.Equals(existingCred.Key, key) &&
            StringComparer.Ordinal.Equals(existingCred.Password, value))
        {
            return new(Task.CompletedTask);
        }

        try
        {
            SecretService* secService = GetSecretService();

            attributes = g_hash_table_new_full(g_str_hash, g_str_equal,
                Marshal.FreeHGlobal, Marshal.FreeHGlobal);

            string fullServiceName = CreateServiceName(_service);
            IntPtr serviceKeyPtr = Marshal.StringToHGlobalAnsi(ServiceAttributeName);
            IntPtr serviceValuePtr = Marshal.StringToHGlobalAnsi(fullServiceName);
            g_hash_table_insert(attributes, serviceKeyPtr, serviceValuePtr);

            if (!string.IsNullOrWhiteSpace(key))
            {
                IntPtr accountKeyPtr = Marshal.StringToHGlobalAnsi(AccountAttributeName);
                IntPtr accountValuePtr = Marshal.StringToHGlobalAnsi(key);
                g_hash_table_insert(attributes, accountKeyPtr, accountValuePtr);
            }

            byte[] secretBytes = Encoding.UTF8.GetBytes(value);
            secretValue = secret_value_new(secretBytes, secretBytes.Length, PlainTextContentType);

            SecretSchema schema = GetSchema();

            bool result = secret_service_store_sync(
                secService,
                ref schema,
                attributes,
                null,
                fullServiceName,
                secretValue,
                IntPtr.Zero,
                out error);

            if (error != null)
            {
                int code = error->code;
                string message = Marshal.PtrToStringAuto(error->message)!;
                throw new InteropException("Failed to store credentials", code, new Exception(message));
            }

            if (!result)
            {
                throw new InteropException("Failed to store credentials", -1);
            }
        }
        finally
        {
            if (attributes != null) g_hash_table_destroy(attributes);
            if (secretValue != null) secret_value_unref(secretValue);
            if (error != null) g_error_free(error);
        }
        
        return new(Task.CompletedTask);
    }

    public unsafe ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        GHashTable* attributes = null;
        GError* error = null;

        try
        {
            SecretService* secService = GetSecretService();

            attributes = CreateSearchQuery(_service, key);

            SecretSchema schema = GetSchema();

            bool result = secret_service_clear_sync(
                secService,
                ref schema,
                attributes,
                IntPtr.Zero,
                out error);

            if (error != null)
            {
                int code = error->code;
                string message = Marshal.PtrToStringAuto(error->message)!;
                throw new InteropException("Failed to erase credentials", code, new Exception(message));
            }

            return new(result);
        }
        finally
        {
            if (attributes != null) g_hash_table_destroy(attributes);
            if (error != null) g_error_free(error);
        }
    }
}