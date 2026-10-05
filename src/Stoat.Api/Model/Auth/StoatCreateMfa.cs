using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Auth;

public class StoatCreateMfa
{
    public class Request
    {
        [JsonPropertyName("password")] public string? Password { get; set; }
    }
    
    public class Response
    {
        [JsonPropertyName("_id")] public string? Id { get; set; }
        [JsonPropertyName("account_id")] public string? AccountId { get; set; }
        [JsonPropertyName("token")] public string? Token { get; set; }
        [JsonPropertyName("validated")] public bool Validated { get; set; }
        [JsonPropertyName("authorised")] public bool Authorised { get; set; }
        [JsonPropertyName("last_totp_code")] public string? LastTotpCode { get; set; }
    }
}