using Stoat.Api.Model;
using Stoat.Api.Model.Auth;

namespace Stoat.Core.Interfaces;

public interface IAuthService
{
    StoatSession? Session { get; }
    bool IsAuthenticated { get; }
    Task<StoatResponse<bool>> LoginAsync(string email, string username, string password, CancellationToken token = default);
    Task<StoatResponse<bool>> LogoutAsync(CancellationToken token = default);

    Task<StoatResponse<bool>> RestoreSessionAsync(CancellationToken token = default);
    Task<StoatResponse<ICollection<SessionData>>> GetAllAsync(CancellationToken token = default);
}