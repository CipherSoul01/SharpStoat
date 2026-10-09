using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using ReactiveUI.Primitives.Signals;
using ReactiveUI.SourceGenerators;
using Stoat.Theme.Controls;
using Stoat.Theme.Controls.Captcha;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel
{
    [ReactiveCommand]
    private async Task Previous()
        => await HostScreen.Router.NavigateBack.Execute();

    [ReactiveCommand]
    private void InvertShowPassword()
        => ShowPassword = !ShowPassword;

    [ReactiveCommand]
    private void ClickLogo()
        => LogoClick += 1;

    [ReactiveCommand]
    private async Task ClickLogin()
    {
        var webview = new HCaptchaView("3daae85e-09ab-4ff6-9f24-e8f4f335e433");

        var app = Application.Current;

        if (app is not null &&
            app.TryGetResource(
                "Background",
                app.ActualThemeVariant,
                out var resource) &&
            resource is ISolidColorBrush brush)
        {
            var color = brush.Color;

            webview.Background = brush;
        }
        

        webview.Completed += (_, x) => Console.WriteLine(x.Token);

        var dialog = new Dialog
        {
            Title = "Welcome to Stoat!",
            Content = webview,
            CardMaxWidth = 500,
            CardHeight = 500
        };

        HostScreen.Dialog = dialog;

        webview.Show();
        dialog.Open();
    }
}