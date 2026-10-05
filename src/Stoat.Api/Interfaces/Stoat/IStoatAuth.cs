using Refit;
using Stoat.Api.Interfaces.Auth;
using Stoat.Api.Model;

namespace Stoat.Api.Interfaces.Stoat;

public interface IStoatAuth : IStoatAccountApi, IStoatSessionApi
{
    [Get("/api/?")]
    Task<ApiResponse<StoatConfig>> GetConfig();
}