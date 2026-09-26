using Avalonia;
using System;
using System.Globalization;
using System.IO;
using Lang.Avalonia;
using Lang.Avalonia.Json;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia.Splat;
using Splat;
using Stoat.Client.View.Layout;
using Stoat.Client.View.Setup;
using Stoat.Client.View.Setup.Auth;
using Stoat.Client.ViewModel.Layout;
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
                    services.AddTransient<IViewFor<AuthViewModel>>((_) => new AuthView());
                    services.AddTransient<IViewFor<LoginViewModel>>((_) => new LoginView());
                    services.AddTransient<IViewFor<SetupLayoutViewModel>>((_) => new SetupLayout());
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
