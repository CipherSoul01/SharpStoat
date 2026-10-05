using Stoat.Api.Interfaces;
using Stoat.Api.Model;

namespace Stoat.Core.Interfaces;

public interface IStoatService
{
    IStoatServerConfiguration Configuration { get; }
    IAuthService Auth { get; init; }
    Task<StoatResponse<bool>> SetBaseAddressAsync(Uri address);
    Task<StoatResponse<bool>> SetupConfigAsync();
}