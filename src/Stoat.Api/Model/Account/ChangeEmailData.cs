using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Account;

public class ChangeEmailData : ChangeBaseData
{
    [JsonPropertyName("email")] public string? Email { get; set; }
}