using Refit;
using Stoat.Api.Model.Auth;

namespace Stoat.Api.Interfaces.Auth;

public interface IStoatSessionApi
{ 
    [Post("/api/auth/session/login")]
    Task<ApiResponse<LoginData.Response?>> LoginAsync(
        [Body] LoginData.Request? request, 
        CancellationToken token = default);
    
    [Post("/api/auth/session/logout")]
    Task<ApiResponse<object?>> LogoutAsync(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);
    
    [Get("/api/auth/session/all")]
    Task<ApiResponse<ICollection<SessionData>>> GetAllAsync(
        [Header("x-session-token")] string sessionToken,
        CancellationToken token = default);

    [Delete("/api/auth/session/all")]
    Task<ApiResponse<object?>> DeleteAllAsync(
        [Header("x-mfa-ticket")] string ticket,
        CancellationToken token = default);
    
    [Delete("/api/auth/session/{id}")]
    Task<ApiResponse<object?>> DeleteAllAsync(
        string id,
        [Header("x-mfa-ticket")] string ticket,
        CancellationToken token = default);

    [Patch("/api/auth/session/{id}")]
    Task<ApiResponse<SessionData>> EditSessionAsync(
        [Header("x-session-token")] string sessionToken,
        string id,
        [Body] SessionData.EditData editData,
        CancellationToken token = default);
}