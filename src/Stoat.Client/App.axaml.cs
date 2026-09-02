using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Stoat.Client.Theme;

namespace Stoat.Client;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ThemeService.Instance.SetupDefaultTheme();
            
            if(Design.IsDesignMode)
                ThemeService.Instance.SetTheme(StoatThemeVariants.Dark);
            
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
        
        
        
    }
}