using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Stoat.Theme.Interactivity;
using Stoat.Theme.Utilities.Server;

namespace Stoat.Theme.Controls.Captcha;

public class HCaptchaView : ContentControl, IDisposable
{
    private readonly NativeWebView _dialog = new();
    private LocalHtmlServer? _server;
    private bool _disposed;

    public event EventHandler<HCaptchaResultArgs>? Completed;
    public event EventHandler<HCaptchaResultArgs>? Expired;
    public event EventHandler<HCaptchaResultArgs>? Error;
    public event EventHandler? Cancelled;

    public HCaptchaView(string siteKey)
    {
        SiteKey = siteKey;
        _dialog.WebMessageReceived += DialogOnWebMessageReceived;
    }

    public string SiteKey { get; init; }

    public new ISolidColorBrush? Background { get; set; } =
        SolidColorBrush.Parse("#fff");

    private string BackgroundHex
    {
        get
        {
            var color = Background!.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    private string Theme
    {
        get
        {
            var color = Background!.Color;

            var r = ToLinear(color.R / 255.0);
            var g = ToLinear(color.G / 255.0);
            var b = ToLinear(color.B / 255.0);

            var luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;

            return luminance > 0.179 ? "light" : "dark";
        }
    }

    public void Show()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Stop();

        var html = HCaptchaValues.Html
            .Replace("@key", SiteKey)
            .Replace("@background", BackgroundHex)
            .Replace("@theme", Theme);

        _dialog.Background = Background;

        _server = new LocalHtmlServer(html);

        _dialog.Navigate(new Uri(_server.Url));

        Content = _dialog;
    }

    public void Stop()
    {
        Content = null;

        _dialog.Stop();
        _server?.Dispose();
        _server = null;
    }

    private void DialogOnWebMessageReceived(
        object? sender,
        WebMessageReceivedEventArgs e)
    {
        if (_disposed)
            return;

        if (e.Body == null)
        {
            OnError(new HCaptchaResultArgs(new HCaptchaResult
            {
                Type = "error",
                Token = "Undefined"
            }));

            return;
        }

        var body = JsonSerializer.Deserialize<HCaptchaResult>(e.Body);

        if (body == null)
            return;

        switch (body.Type)
        {
            case "completed":
                OnCompleted(new HCaptchaResultArgs(body));
                Stop();
                break;

            case "expired":
                OnExpired(new HCaptchaResultArgs(body));
                Stop();
                break;

            case "error":
                OnError(new HCaptchaResultArgs(body));
                Stop();
                break;

            case "cancelled":
                Stop();
                Cancelled?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private void OnCompleted(HCaptchaResultArgs e)
        => Completed?.Invoke(this, e);

    private void OnExpired(HCaptchaResultArgs e)
        => Expired?.Invoke(this, e);

    private void OnError(HCaptchaResultArgs e)
        => Error?.Invoke(this, e);

    private static double ToLinear(double value)
    {
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        Stop();

        _dialog.WebMessageReceived -= DialogOnWebMessageReceived;
        
        GC.SuppressFinalize(this);
    }
}



internal class HCaptchaValues
{
    internal const string Html = """
                               <!DOCTYPE html>
                               <html lang="pt-BR">
                               <head>
                                   <meta charset="UTF-8">
                                   <meta name="viewport" content="width=device-width, initial-scale=1">

                                   <style>
                                       :root {
                                           color-scheme: @theme;
                                           --destructive: #dc2626;
                                           --destructive-hover: #b91c1c;
                                       }

                                       @media (prefers-color-scheme: dark) {
                                           :root {
                                               --destructive: #f87171;
                                               --destructive-hover: #ef4444;
                                           }
                                       }

                                       body {
                                           margin: 0;
                                           padding: 20px;
                                           box-sizing: border-box;
                                           background: @background;
                                           color-scheme: @theme;
                                           display: flex;
                                           flex-direction: column;
                                           justify-content: center;
                                           align-items: center;
                                           min-height: 100vh;
                                           font-family: Arial, sans-serif;
                                       }

                                       .cancel-button {
                                           margin-top: 16px;
                                           padding: 9px 24px;
                                           border: 1px solid var(--destructive);
                                           border-radius: 6px;
                                           background: transparent;
                                           color: var(--destructive);
                                           font-size: 14px;
                                           cursor: pointer;
                                           transition: background 150ms ease,
                                                       color 150ms ease;
                                       }

                                       .cancel-button:hover {
                                           background: var(--destructive);
                                           color: white;
                                       }

                                       .cancel-button:focus-visible {
                                           outline: 2px solid var(--destructive);
                                           outline-offset: 3px;
                                       }
                                   </style>

                                   <script src="https://js.hcaptcha.com/1/api.js" async defer></script>
                               </head>
                               <body>
                                   <div class="h-captcha"
                                        data-sitekey="@key"
                                        data-theme="@theme"
                                        data-callback="onVerified"
                                        data-expired-callback="onExpired"
                                        data-error-callback="onError">
                                   </div>

                                   <button class="cancel-button" onclick="onCancelled()">
                                       Cancel
                                   </button>

                                   <script>
                                       function sendMessage(type, token = "") {
                                           const message = JSON.stringify({
                                               type: type,
                                               token: token
                                           });

                                           invokeCSharpAction(message);
                                       }

                                       function onVerified(token) {
                                           sendMessage("completed", token);
                                       }

                                       function onExpired() {
                                           sendMessage("expired");
                                       }

                                       function onError(error) {
                                           sendMessage("error", String(error ?? ""));
                                       }

                                       function onCancelled() {
                                           sendMessage("cancelled");
                                       }
                                   </script>
                               </body>
                               </html>
                               """;
}