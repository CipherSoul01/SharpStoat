using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Stoat.Client;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var plash = new SplashWindow();
            
            desktop.MainWindow = plash;
            
            plash.Show();

            await plash.InitializeServicesAsync();
            
            var mainWindow = new MainWindow();
            
            desktop.MainWindow = mainWindow;
            
            mainWindow.Show();
            plash.Close();
            
            //desktop.MainWindow = new SplashWindow();
        }

        base.OnFrameworkInitializationCompleted();
        
        
        
    }
}