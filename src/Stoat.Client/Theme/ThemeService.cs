using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Stoat.Client.Theme;

public sealed class ThemeService
{
    public static ThemeService Instance { get; } = new();
    
    public ThemeVariant Current =>
        Application.Current!.ActualThemeVariant;

    public void SetTheme(ThemeVariant theme)
    {
        Application.Current!.RequestedThemeVariant = theme;
    }

    public void SetupDefaultTheme()
    {
        var settings = Application.Current!.PlatformSettings;
        var colors = settings?.GetColorValues();
        
        bool isDark = colors?.ThemeVariant == PlatformThemeVariant.Dark;
        
        Application.Current.RequestedThemeVariant = isDark ? StoatThemeVariants.Dark : StoatThemeVariants.Light;
    }
}