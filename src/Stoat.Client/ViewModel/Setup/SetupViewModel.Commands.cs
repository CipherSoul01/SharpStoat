using ReactiveUI.SourceGenerators;
using SkiaSharp;
using Stoat.Client.Theme;
using Stoat.Client.ViewModel.Setup.Auth;

namespace Stoat.Client.ViewModel.Setup;

public partial class SetupViewModel
{
    [ReactiveCommand]
    private void Setup()
    {
        IsDarkTheme = ThemeService.Instance.Current == StoatThemeVariants.Dark;

        Router.Navigate.Execute(new AuthViewModel(this, HostScreen));
    }

    [ReactiveCommand]
    private void ChangeTheme()
    {
        var theme = IsDarkTheme ? StoatThemeVariants.Light :  StoatThemeVariants.Dark;
        ThemeService.Instance.SetTheme(theme);
        
        IsDarkTheme = !IsDarkTheme;
    }
}