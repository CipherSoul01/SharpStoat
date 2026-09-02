using Avalonia;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Lang.Avalonia;
using Lang.Avalonia.Json;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Avalonia.Splat;
using Splat;
using Stoat.Client.View.Setup;
using Stoat.Client.View.Setup.Auth;
using Stoat.Client.ViewModel.Setup;
using Stoat.Client.ViewModel.Setup.Auth;
using AuthView = Stoat.Client.View.Setup.Auth.AuthView;

namespace Stoat.Client;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUIWithMicrosoftDependencyResolver(
                services =>
                {
                    AppLocator.CurrentMutable.Register(() => new AuthView(), typeof(IViewFor<AuthViewModel>));
                    AppLocator.CurrentMutable.Register(()  => new SetupView(), typeof(IViewFor<SetupViewModel>));
                    AppLocator.CurrentMutable.Register(() => new LoginView(), typeof(IViewFor<LoginViewModel>));
                },
                withResolver: sp =>
                {
                    I18nManager.Instance.Register(new JsonLangPlugin()
                        {
                            ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n"),
                        }, 
                        new CultureInfo("pt-BR"));
                })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
