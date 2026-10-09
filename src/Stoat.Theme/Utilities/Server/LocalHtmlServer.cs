using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Stoat.Theme.Utilities.Server;

public sealed class LocalHtmlServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly string _html;
    private readonly CancellationTokenSource _cts = new();
    private Task? _serverTask;

    public string Url { get; }

    public LocalHtmlServer(string html)
    {
        _html = html;

        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();

        var endpoint = (IPEndPoint)_listener.LocalEndpoint;
        Url = $"http://127.0.0.1:{endpoint.Port}/";

        _serverTask = RunAsync(_cts.Token);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(
                    cancellationToken);

                await HandleRequestAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task HandleRequestAsync(
        TcpClient client,
        CancellationToken cancellationToken)
    {
        var stream = client.GetStream();

        using var reader = new StreamReader(
            stream,
            Encoding.ASCII,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var requestLine = await reader.ReadLineAsync(cancellationToken);

        if (requestLine is null)
            return;

        string? header;

        do
        {
            header = await reader.ReadLineAsync(cancellationToken);
        }
        while (!string.IsNullOrEmpty(header));

        var isRootRequest =
            requestLine.StartsWith("GET / HTTP/", StringComparison.Ordinal);

        var body = isRootRequest
            ? Encoding.UTF8.GetBytes(_html)
            : [];

        var status = isRootRequest ? "200 OK" : "404 Not Found";
        var contentType = "text/html; charset=utf-8";

        var responseHeaders = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {status}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            "Cache-Control: no-store\r\n" +
            "Connection: close\r\n\r\n");

        await stream.WriteAsync(responseHeaders, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();

        _cts.Dispose();
    }
}