using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Media;
using Stoat.Theme.Interactivity;
using Stoat.Theme.Utilities.Server;

namespace Stoat.Theme.Controls.Captcha;

public class HCaptchaView : ContentControl, IDisposable
{
    
    private string _baseHtml = """
                               <!DOCTYPE html>
                               <html lang="pt-BR">
                               <head>
                                   <meta charset="UTF-8">
                                   <meta name="viewport" content="width=device-width, initial-scale=1">
                               
                                   <style>
                                       body {
                                           margin: 0;
                                           background: @background;
                                           display: flex;
                                           justify-content: center;
                                           align-items: center;
                                           min-height: 100vh;
                                       }
                                   </style>
                               
                                   <script src="https://js.hcaptcha.com/1/api.js" async defer></script>
                               </head>
                               <body>
                                   <div class="h-captcha"
                                        data-sitekey="@key"
                                        data-callback="onVerified"
                                        data-expired-callback="onExpired"
                                        data-error-callback="onError">
                                   </div>
                               
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
                                   </script>
                               </body>
                               </html>
                               """;
    
    private NativeWebView _dialog = new();
    private LocalHtmlServer? _server;
    
    public event EventHandler<HCaptchaResultArgs>? Completed;
    public event EventHandler<HCaptchaResultArgs>? Expired; 
    public event EventHandler<HCaptchaResultArgs>? Error;
    
    public HCaptchaView(string siteKey)
    {
        SiteKey = siteKey;
        
        _dialog.WebMessageReceived += DialogOnWebMessageReceived;
    }

    public string SiteKey { get; init; }

    public new ISolidColorBrush? Background { get; set; } = SolidColorBrush.Parse("#fff");

    private string BackgroundHex
    {
        get
        {
            var color = Background.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
    

    public void Show()
    {
        var html = _baseHtml.Replace("@key", SiteKey)
            .Replace("@background", BackgroundHex);
        
        if(_server != null)
            _server.Dispose();
        
        _server = new LocalHtmlServer(html);
        
        _dialog.Navigate(new Uri(_server.Url));
        
        Content = _dialog;
    }

    private void Stop()
    {
        if(_server == null)
            return;
        
        Content = null;
        _dialog.Stop();
        _server?.Dispose();
        _server = null;
    }
    
    public void Dispose()
    {
        Content = null;
        _dialog.Stop();
        
        if(_server != null)
            _server.Dispose();
    }   
    
    private void DialogOnWebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
    {
        if (e.Body == null)
        {
            OnError(new HCaptchaResultArgs(new HCaptchaResult()
            {
                 Type = "error",
                 Token = "Undenifed"
            }));
            
            return;
        } 
        
        var body = JsonSerializer.Deserialize<HCaptchaResult>(e.Body);
        
        if(body == null)
            return;
        
        switch (body.Type)
        {
            case "completed":
                OnCompleted(new HCaptchaResultArgs(body));
                break;
            case "expired":
                OnExpired(new HCaptchaResultArgs(body));
                break;
            case "error":
                OnError(new HCaptchaResultArgs(body));
                break;
        }
    }
    
    
    private void OnCompleted(HCaptchaResultArgs e)
        => Completed?.Invoke(this, e);

    private void OnExpired(HCaptchaResultArgs e)
        => Expired?.Invoke(this, e);
    
    private void OnError(HCaptchaResultArgs e)
        => Error?.Invoke(this, e);
}