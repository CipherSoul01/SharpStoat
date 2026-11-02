using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Lang.Avalonia;
using Lucide.Avalonia;
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
        var result = await _stoat.Auth.LoginAsync(
            Email,
            "Stoat for Desktop (SharpStoat)",
            Password);

        if (!result.IsSuccess)
        {
            var localization = I18nManager.Instance;

            var content = new TextBlock()
            {
                Text = localization.GetResource(
                    $"Localization.Errors.{result.ErrorMessage}") 
            };
            
            content.Classes.AddRange(["small", "muted"]);

            var dialog = new Dialog
            {
                Title = localization.GetResource(
                    "Localization.Setup.Auth.SignIn.Failure"),
                Content = content, 
                CardMinWidth = 500,
                CardMaxWidth = 700,
                CardMaxHeight = 600,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            dialog.Classes.Add("error");

            HostScreen.Dialog = dialog;
            dialog.Open();

            return;
        }

        /*var captcha = new HCaptchaView(_stoat.Configuration.Config!.Features!.Captcha!.Key!);

        var app = Application.Current;

        if (app is not null &&
            app.TryGetResource(
                "Background",
                app.ActualThemeVariant,
                out var resource) &&
            resource is ISolidColorBrush brush)
        {
            captcha.Background = brush;
        }

        var dialog = new Dialog
        {
            Title = "Welcome to Stoat!",
            Content = captcha,
            CloseOnClickOutside = false,
            CloseOnEscape = false,
            ShowCloseButton = false,
            CardMaxWidth = 700,
            CardHeight = 600,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(10)
        };

        void CloseCaptcha()
        {
            dialog.Content = null;

            captcha.Stop();
            dialog.Close();
            captcha.Dispose();
        }

        captcha.Completed += (_, x) =>
        {
            CloseCaptcha();
            Console.WriteLine(x.Token);
        };

        captcha.Error += (_, _) => CloseCaptcha();
        captcha.Expired += (_, _) => CloseCaptcha();
        captcha.Cancelled += (_, _) => CloseCaptcha();

        HostScreen.Dialog = null;
        HostScreen.Dialog = dialog;

        dialog.Open();
        captcha.Show();*/
    }
}