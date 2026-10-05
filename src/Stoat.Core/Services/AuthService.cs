using System.Security.Cryptography;
using System.Text.Json;
using Stoat.Api.Interfaces.Auth;
using Stoat.Api.Interfaces.Stoat;
using Stoat.Api.Model;
using Stoat.Api.Model.Auth;
using Stoat.Core.Interfaces;
using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services;

public class AuthService : IAuthService
{
    private readonly IStoatAuth _session;
    private readonly IStoatMfa _mfa;
    private readonly ISecretStore _store;
    
    public StoatSession? Session { get; private set; }
    
    public bool IsAuthenticated => Session is not null;
    
    public AuthService(IStoatAuth @session, ISecretStore @store, IStoatMfa @mfa)
    {
        _session = @session;
        _store = @store;
        _mfa = mfa;
    }

    public async Task<StoatResponse<bool>> LoginAsync(string email, string username, string password, CancellationToken token = default)
    {
        var request = new LoginData.Request()
        {
            Email = email,
            Password = password,
            FriendlyName = username
        };

        token.ThrowIfCancellationRequested();
        
        var response = await _session.LoginAsync(request, token);

        if (!response.IsSuccessful)
            return new StoatResponse<bool>(false);

        if (response.HasContent)
        {
            Session = new StoatSession(response.Content);
            
            using var aes = Aes.Create();
            
            var text = await EncryptAsync(Session, aes.Key, aes.IV);

            if(!Directory.Exists(StoatDir.Local))
                Directory.CreateDirectory(StoatDir.Local);
            
            await File.WriteAllBytesAsync(Path.Combine(StoatDir.Local, "session.bin"), text, token);
            
            await _store.SetAsync("stoat-key", Convert.ToBase64String(aes.Key), token);
            await _store.SetAsync("stoat-iv", Convert.ToBase64String(aes.IV), token);
        }
        
        return new StoatResponse<bool>(response.HasContent);
    }

    public async Task<StoatResponse<bool>> LogoutAsync(CancellationToken token = default)
    {
        if(Session == null || Session.Token == null)
            return new StoatResponse<bool>([ "Session is null" ]);
        
        token.ThrowIfCancellationRequested();
        
        var response = await _session.LogoutAsync(Session.Token, token);

        if (!response.IsSuccessful)
            return new StoatResponse<bool>([ response.Error?.Message ?? "Unknown error" ]);

        var sessionPath = Path.Combine(StoatDir.Local, "session.bin");
        
        if(File.Exists(sessionPath))
            File.Delete(sessionPath);
        
        await _store.DeleteAsync("stoat-key", token);
        await _store.DeleteAsync("stoat-iv", token);
        
        return new StoatResponse<bool>(true);
    }

    public async Task<StoatResponse<bool>> RestoreSessionAsync(CancellationToken token = default)
    {
        if (Session is not null)
            return new(true);

        token.ThrowIfCancellationRequested();

        var sessionPath = Path.Combine(StoatDir.Local, "session.bin");

        if (!File.Exists(sessionPath))
            return new StoatResponse<bool>(["Session not found"]);

        try
        {
            var keyString = await _store.GetAsync("stoat-key", token);
            var ivString = await _store.GetAsync("stoat-iv", token);

            if (string.IsNullOrWhiteSpace(keyString?.Password) ||
                string.IsNullOrWhiteSpace(ivString?.Password))
            {
                return new StoatResponse<bool>([
                    "Session encryption keys not found"
                ]);
            }

            var key = Convert.FromBase64String(keyString.Password);
            var iv = Convert.FromBase64String(ivString.Password);

            var encrypted = await File.ReadAllBytesAsync(sessionPath, token);

            Session = await DecryptAsync(encrypted, key, iv);

            return new(true);
        }
        catch (CryptographicException ex)
        {
            return new StoatResponse<bool>([
                $"Failed to decrypt session: {ex.Message}"
            ]);
        }
        catch (JsonException ex)
        {
            return new StoatResponse<bool>([
                $"Failed to deserialize session: {ex.Message}"
            ]);
        }
    }

    public async Task<StoatResponse<ICollection<SessionData>>> GetAllAsync(CancellationToken token = default)
    {
        if(Session == null || Session.Token == null)
            return new StoatResponse<ICollection<SessionData>>([ "Session is null" ]);
        
        token.ThrowIfCancellationRequested();

        var response = await _session.GetAllAsync(Session.Token, token);
        
        if (!response.IsSuccessful || !response.HasContent)
            return new StoatResponse<ICollection<SessionData>>([response.Error?.Message ?? "Unknown error"]);

        return new StoatResponse<ICollection<SessionData>>(response.Content);
    }

    private async Task<byte[]> EncryptAsync(StoatSession data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();

        aes.Key = key;
        aes.IV = iv;

        await using var output = new MemoryStream();

        await using (var crypto = new CryptoStream(
                         output,
                         aes.CreateEncryptor(),
                         CryptoStreamMode.Write,
                         leaveOpen: true))
        await using (var writer = new StreamWriter(crypto))
        {
            await writer.WriteAsync(JsonSerializer.Serialize(data));
        }

        return output.ToArray();
    }
    
    private async Task<StoatSession> DecryptAsync(
        byte[] encrypted,
        byte[] key,
        byte[] iv)
    {
        using var aes = Aes.Create();

        aes.Key = key;
        aes.IV = iv;

        await using var input = new MemoryStream(encrypted);

        await using var crypto = new CryptoStream(
            input,
            aes.CreateDecryptor(),
            CryptoStreamMode.Read);

        using var reader = new StreamReader(crypto);

        var json = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<StoatSession>(json)
               ?? throw new InvalidOperationException("Failed to deserialize the stored session.");
    }
}