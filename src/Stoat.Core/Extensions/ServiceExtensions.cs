using Microsoft.Extensions.DependencyInjection;
using Stoat.Core.Interfaces;
using Stoat.Core.Interfaces.Credential;
using Stoat.Core.Services;
using Stoat.Core.Services.Storage;

namespace Stoat.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddStoatCore(this IServiceCollection services)
    {
        services.AddSingleton<ISecretStore, OsStorage>();
        services.AddSingleton<IStoatService, StoatService>();
        services.AddSingleton<IAuthService, AuthService>();
        
        return services;
    }
}