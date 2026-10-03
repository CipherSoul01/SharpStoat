using Stoat.Api.Interfaces.Stoat;
using Stoat.Api.Model;
using Stoat.Api.Model.Auth;
using Stoat.Core.Interfaces;
using Stoat.Core.Interfaces.Credential;

namespace Stoat.Core.Services;

public class AuthService : ISessionService
{
    private readonly IStoatAuth _session;
    private readonly ISecretStore _store;
    
    public StoatSession? Session { get; private set; }
    
    public bool IsAuthenticated => Session is not null;
    
    public AuthService(IStoatAuth @session, ISecretStore @store)
    {
        _session = @session;
        _store = @store;
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

        return new StoatResponse<bool>(true);
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
}