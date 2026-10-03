using Refit;
using Stoat.Api.Model.Account;

namespace Stoat.Api.Interfaces.Auth;

public interface IStoatAccountApi
{
    [Patch("/auth/account/change/password")]
    Task<ApiResponse<object?>> ChangePasswordAsync(
        [Body] ChangePasswordData data,
        CancellationToken token = default);

    [Patch("/auth/account/change/email")]
    Task<ApiResponse<object?>> ChangeEmailAsync(
        [Body] ChangeEmailData data,
        CancellationToken token = default);
}