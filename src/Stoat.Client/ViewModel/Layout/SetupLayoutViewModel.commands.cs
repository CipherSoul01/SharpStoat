using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Styling;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Layout;

public partial class SetupLayoutViewModel
{
    [ReactiveCommand]
    private async Task Setup()
    {
        var variant = App.Current.ActualThemeVariant;

        IsDarkTheme = variant == ThemeVariant.Dark;
    }

    [ReactiveCommand]
    private void ChangeTheme()
    {
        var variant = App.Current.ActualThemeVariant;

        App.Current.RequestedThemeVariant = variant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        
        IsDarkTheme = !IsDarkTheme;
    }

    [ReactiveCommand]
    private void OpenAbout()
        => OpenBrowser(_stoat.Configuration.Config?.Features?.LegalLinks?.Guidelines ?? "");

    [ReactiveCommand]
    private void OpenTermsOfService()
        => OpenBrowser(_stoat.Configuration.Config?.Features?.LegalLinks?.Terms ?? "");

    [ReactiveCommand]
    private void OpenPrivatePolicy()
        => OpenBrowser(_stoat.Configuration.Config?.Features?.LegalLinks?.PrivacyPolicy ?? "");
    
    private void OpenBrowser(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = url,
                UseShellExecute = false
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments = url,
                UseShellExecute = false
            });
        } 
    }
}