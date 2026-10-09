using System.Text.Json.Serialization;

namespace Stoat.Theme.Interactivity;

public class HCaptchaResultArgs : EventArgs
{
    public bool IsSuccess { get; init; }
    public string? Type { get; init; }
    public string? Token { get; init; }

    public HCaptchaResultArgs(HCaptchaResult result)
    {
        IsSuccess = result.Type switch
        {
            "completed" => true,
            _ => false
        };
        
        Type = result.Type;
        Token = result.Token;
    }
}

public class HCaptchaResult
{
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("token")] public string? Token { get; set; }
}