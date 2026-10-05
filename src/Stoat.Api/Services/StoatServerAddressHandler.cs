using Stoat.Api.Interfaces;

namespace Stoat.Api.Services;

public class StoatServerAddressHandler : DelegatingHandler
{
    private readonly IStoatServerConfiguration _configuration;

    public StoatServerAddressHandler(IStoatServerConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var baseAddress =
            _configuration.BaseAddress ??
            new Uri("https://stoat.chat/");

        request.Headers.Referrer = baseAddress;

        request.RequestUri = new Uri(
            baseAddress,
            request.RequestUri!);

        return base.SendAsync(request, cancellationToken);
    }
}