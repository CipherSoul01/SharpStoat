using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Auth;

public class SessionData
{
    [JsonPropertyName("_id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    
    public class EditData
    {
        [JsonPropertyName("friendly_name")] public string Name { get; set; }
    }
}