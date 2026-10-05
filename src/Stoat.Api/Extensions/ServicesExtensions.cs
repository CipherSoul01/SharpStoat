using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using Stoat.Api.Interfaces;
using Stoat.Api.Interfaces.Auth;
using Stoat.Api.Interfaces.Stoat;
using Stoat.Api.Services;

namespace Stoat.Api.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddStoatApi(this IServiceCollection services)
    {
        services.AddTransient<StoatServerAddressHandler>();
        services.AddSingleton<IStoatServerConfiguration, StoatServerConfiguration>();
        
        services.AddRefitClient<IStoatAuth>()
            .ConfigureStoatHttpClient();

        services.AddRefitClient<IStoatAccountApi>()
            .ConfigureStoatHttpClient();
        
        services.AddRefitClient<IStoatSessionApi>()
            .ConfigureStoatHttpClient();
        
        services.AddRefitClient<IStoatMfa>()
            .ConfigureStoatHttpClient();
        
        return services;
    }

    private static IHttpClientBuilder ConfigureStoatHttpClient(this IHttpClientBuilder builder)
    {
        builder.ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri("https://stoat.chat/");
            c.Timeout = TimeSpan.FromSeconds(20);
            
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64; rv:157.0) Gecko/20100101 Firefox/157.0");
        })
        .AddHttpMessageHandler<StoatServerAddressHandler>()
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            
            options.Retry.Delay = TimeSpan.FromSeconds(1);

            options.Retry.BackoffType =
                DelayBackoffType.Exponential;

            options.Retry.UseJitter = true;
        });
        
        return builder;
    }
}