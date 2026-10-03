using Stoat.Api.Model.Auth;

namespace Stoat.Api.Model;

public class StoatSession
{
    public string? Token { get; set; }
    
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? LeatSeen { get; set; }
    
    public StoatSubscription? Subscription { get; set; }

    public StoatSession(LoginData.Response data)
    {
        Token = data.Token;
        Id = data.Id;
        Name = data.Name;
        LeatSeen = data.LastSeen;
        Subscription = new StoatSubscription()
        {
            Endpoint = data.Subscription?.Endpoint,
            P256Dh = data.Subscription?.P256Dh,
            Auth = data.Subscription?.Auth
        };
    }
}

public class StoatSubscription
{
    public string? Endpoint { get; set; }
    public string? P256Dh { get; set; }
    public string? Auth { get; set; }
}