using Stoat.Core.Image.Pipeline;
using Stoat.Core.Interfaces.Transport;

namespace Stoat.Core.Image.Transport;

public sealed class HttpImageTransport : IImageTransport {
    private readonly HttpClient _httpClient;

    public HttpImageTransport(HttpClient httpClient) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<Stream?> GetAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default) {
        if (!Uri.TryCreate(request.Source, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return null;

        var response = await _httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        try {
            response.EnsureSuccessStatusCode();
            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            return new HttpResponseStream(responseStream, response);
        }
        catch {
            response.Dispose();
            throw;
        }
    }
}