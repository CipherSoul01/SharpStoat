using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Account;

public class ChangeBaseData
{
    [JsonPropertyName("current_password")]
    public string? CurrentPassword { get; set; }
}