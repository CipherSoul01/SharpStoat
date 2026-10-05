using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Mfa;

public class GenerateTotpData
{
    [JsonPropertyName("secret")] public string? Secret { get; set; }
}