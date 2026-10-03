using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using Stoat.Api.Interfaces.Stoat;

namespace Stoat.Api.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddStoatApi(this IServiceCollection services)
    {
        services.AddRefitClient<IStoatAuth>()
            .ConfigureStoatHttpClient();
        
        return services;
    }

    private static IHttpClientBuilder ConfigureStoatHttpClient(this IHttpClientBuilder builder)
    {
        builder.ConfigureHttpClient(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(20);
            
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64; rv:157.0) Gecko/20100101 Firefox/157.0");
            
            c.DefaultRequestHeaders.Referrer = new Uri("https://stoat.chat/app");
        })
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