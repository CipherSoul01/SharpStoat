using Stoat.Api.Model;
using Stoat.Api.Model.Auth;

namespace Stoat.Core.Interfaces;

public interface IAuthService
{
    StoatSession? Session { get; }
    bool IsAuthenticated { get; }
    Task<StoatResponse> LoginAsync(string email, string service_name, string password, CancellationToken token = default);
    Task<StoatResponse> LogoutAsync(CancellationToken token = default);

    Task<StoatResponse> RestoreSessionAsync(CancellationToken token = default);
    Task<StoatResponse<ICollection<SessionData>>> GetAllAsync(CancellationToken token = default);
}