using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Mfa;

public class EnableTotpData
{
    [JsonPropertyName("password")] public string? Password { get; set; }
}