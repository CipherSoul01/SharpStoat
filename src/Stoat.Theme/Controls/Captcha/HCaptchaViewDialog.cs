using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Media;
using Stoat.Theme.Interactivity;
using Stoat.Theme.Utilities.Server;

namespace Stoat.Theme.Controls.Captcha;

public class HCaptchaViewDialog : IDisposable
{
    private readonly string _siteKey;
    
    private NativeWebDialog _webDialog = new();
    private LocalHtmlServer? _server;
    
    public ISolidColorBrush? Background { get; set; } = SolidColorBrush.Parse("#fff");
    
    private string _backgroundHex
    {
        get
        {
            var color = Background.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
    
    private string Theme
    {
        get
        {
            if (Background == null)
                return "light";
            
            var color = Background.Color;

            var r = ToLinear(color.R / 255.0);
            var g = ToLinear(color.G / 255.0);
            var b = ToLinear(color.B / 255.0);

            var luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;

            return luminance > 0.179 ? "light" : "dark";
        }
    }
    
    public event EventHandler<HCaptchaResultArgs>? Completed;
    public event EventHandler<HCaptchaResultArgs>? Expired; 
    public event EventHandler<HCaptchaResultArgs>? Error;

    public HCaptchaViewDialog(string siteKey)
    {
        _siteKey = siteKey;
        
        _webDialog.Title = "HCaptcha";
        _webDialog.WebMessageReceived += DialogOnWebMessageReceived;
    }

    public string Key => _siteKey;

    public void Show(TopLevel? owner = null)
    {
        var html = HCaptchaValues.Html 
            .Replace("@key", _siteKey)
            .Replace("@background", _backgroundHex)
            .Replace("@theme", Theme);

        _webDialog.DefaultBackground = Background.Color;
        
        _server?.Dispose();
        _server = new LocalHtmlServer(html);
        
        if(owner != null)
            _webDialog.Show(owner);
        else
            _webDialog.Show();
        
        _webDialog.Navigate(new Uri(_server.Url));
    }
    
    public void Stop()
    {
        _webDialog.Stop();
        _webDialog.Close();

        _server?.Dispose();
        _server = null;
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
        
        Stop();
    }

    private void OnCompleted(HCaptchaResultArgs e)
        => Completed?.Invoke(this, e);

    private void OnExpired(HCaptchaResultArgs e)
        => Expired?.Invoke(this, e);

    private void OnError(HCaptchaResultArgs e)
        => Error?.Invoke(this, e); 
   
    private static double ToLinear(double value)
        => value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    
    public void Dispose()
    {
        Stop();
        
        _webDialog.WebMessageReceived -= DialogOnWebMessageReceived;
        
        _webDialog.Dispose();
    }
}