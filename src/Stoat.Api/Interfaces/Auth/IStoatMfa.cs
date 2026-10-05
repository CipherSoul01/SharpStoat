using Refit;
using Stoat.Api.Model.Auth;
using Stoat.Api.Model.Mfa;

namespace Stoat.Api.Interfaces.Auth;

public interface IStoatMfa
{
    [Put("/auth/mfa/ticket")]
    Task<ApiResponse<StoatCreateMfa.Response>> Create(
        [Header("x-session-token")] string sessionToken,
        [Body] StoatCreateMfa.Request request,
        CancellationToken token = default);
    
    [Get("/auth/mfa/")]
    Task<ApiResponse<StoatCreateMfa?>> Status(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Post("/auth/mfa/recovery")]
    Task<ApiResponse<ICollection<string>>> FetchRecovery(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Patch("/auth/mfa/recovery")]
    Task<ApiResponse<ICollection<string>>> GenerateRecovery(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Get("/auth/mfa/methods")]
    Task<ApiResponse<ICollection<string>>> GetMethods(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Put("/auth/mfa/totp")]
    Task<ApiResponse<EnableTotpData>> EnabledTotp(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Post("/auth/mfa/totp")]
    Task<ApiResponse<GenerateTotpData>> GenerateTotp(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Delete("/auth/mfa/totp")]
    Task<ApiResponse<object?>> DisableTotp(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);
}