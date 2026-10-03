using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Auth;

public class LoginData
{
    public class Request
    {
        [JsonPropertyName("email")] public string? Email { get; set; }
        [JsonPropertyName("password")] public string? Password { get; set; }
        [JsonPropertyName("friendly_name")] public string? FriendlyName { get; set; }
    }
    
    public class Response
    {
        [JsonPropertyName("result")] public string? Result { get; set; }
        [JsonPropertyName("_id")] public string? Id { get; set; }
        
        [JsonPropertyName("user_id")] public string? UserId { get; set; }
        [JsonPropertyName("token")] public string? Token { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("last_seen")] public DateTime? LastSeen { get; set; }
        [JsonPropertyName("subscription")] public SubscriptionData? Subscription { get; set; }
        
        public class SubscriptionData
        {
            [JsonPropertyName("endpoint")] public string? Endpoint { get; set; }
            [JsonPropertyName("p256dh")] public string? P256Dh { get; set; }
            [JsonPropertyName("auth")] public string? Auth { get; set; }
        }
    }
}