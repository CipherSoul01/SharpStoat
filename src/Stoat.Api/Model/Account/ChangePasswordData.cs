using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Account;

public class ChangePasswordData : ChangeBaseData
{
    [JsonPropertyName("password")] public string? Password { get; set; }
}