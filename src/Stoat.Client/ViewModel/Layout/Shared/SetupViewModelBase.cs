using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Styling;
using Lang.Avalonia;
using Lucide.Avalonia;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;
using Stoat.Client.ViewModel.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Layout.Shared;

public partial class SetupViewModelBase : MainScreenRoutableViewModelBase
{
    protected readonly IStoatService Stoat;
    
    [Reactive]
    private string? _language;
    
    [Reactive] 
    private bool _isDarkTheme;
    
    private ObservableAsPropertyHelper<LucideIconKind>?  _kindThemeHelper;
    
    public SetupViewModelBase(IMainScreen hostScreen, IStoatService stoat) : base(hostScreen)
    {
        Stoat = stoat;
    }

    public override string? UrlPathSegment => null;
    
    public LucideIconKind KindTheme
        => _kindThemeHelper?.Value ?? LucideIconKind.Moon;

    protected override void SetupRx()
    {
        _kindThemeHelper = this.WhenAnyValue(x => x.IsDarkTheme)
            .Select(x => x ? LucideIconKind.Moon : LucideIconKind.Sun)
            .ToProperty(this, x => x.KindTheme); 
    }

    [ReactiveCommand]
    private async Task Setup()
    {
        var variant = App.Current.ActualThemeVariant;

        IsDarkTheme = variant == ThemeVariant.Dark;

        var lang = I18nManager.Instance.Culture;
        

        Language = lang!.DisplayName.Length > 0
            ? string.Concat(char.ToUpper(lang.DisplayName[0]), lang.DisplayName[1..].ToLower())
            : lang.DisplayName;
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
        => OpenBrowser(Stoat.Configuration.Config?.Features?.LegalLinks?.Guidelines ?? "");

    [ReactiveCommand]
    private void OpenTermsOfService()
        => OpenBrowser(Stoat.Configuration.Config?.Features?.LegalLinks?.Terms ?? "");

    [ReactiveCommand]
    private void OpenPrivatePolicy()
        => OpenBrowser(Stoat.Configuration.Config?.Features?.LegalLinks?.PrivacyPolicy ?? "");
    
    private void OpenBrowser(string url)
    {
        if(string.IsNullOrEmpty(url))
            return;
        
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