using System.Threading.Tasks;
using Avalonia.Styling;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Layout;

public partial class SetupLayoutViewModel
{
    [ReactiveCommand]
    private async Task Setup()
    {
        /*IsDarkTheme = ThemeService.Instance.Current == StoatThemeVariants.Dark;*/
        var variant = App.Current.ActualThemeVariant;

        IsDarkTheme = variant == ThemeVariant.Dark;
    }

    [ReactiveCommand]
    private void ChangeTheme()
    {
        /*var theme = IsDarkTheme ? StoatThemeVariants.Light :  StoatThemeVariants.Dark;
        ThemeService.Instance.SetTheme(theme);*/
        var variant = App.Current.ActualThemeVariant;

        App.Current.RequestedThemeVariant = variant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        
        IsDarkTheme = !IsDarkTheme;
    }
}