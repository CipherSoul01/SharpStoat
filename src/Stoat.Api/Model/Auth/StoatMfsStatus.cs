using System.Text.Json.Serialization;

namespace Stoat.Api.Model.Auth;

public class StoatMfsStatus
{
    [JsonPropertyName("email_otp")] public bool EmailOtp { get; set; }
    [JsonPropertyName("trusted_handover")] public bool TrustedHandover { get; set; }
    [JsonPropertyName("email_mfa")] public bool EmailMfa { get; set; }
    [JsonPropertyName("totp_mfa")] public bool TotpMfa { get; set; }
    [JsonPropertyName("security_key_mfa")] public bool SecurityKeyMfa { get; set; }
    [JsonPropertyName("recovery_active")] public bool RecoveryActive { get; set; }
}