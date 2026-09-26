using Lucide.Avalonia;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Stoat.Client.ViewModel.Layout;

public partial class SetupLayoutViewModel
{
    private ObservableAsPropertyHelper<LucideIconKind>?  _kindThemeHelper;
    public LucideIconKind KindTheme
        => _kindThemeHelper?.Value ?? LucideIconKind.Moon;
    
    private bool _isDarkTheme = true;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => this.RaiseAndSetIfChanged(ref _isDarkTheme, value);
    }
    
    protected override void SetupRx()
    {
        _kindThemeHelper = this.WhenAnyValue(x => x.IsDarkTheme)
            .Select(x => x ? LucideIconKind.Moon : LucideIconKind.Sun)
            .ToProperty(this, x => x.KindTheme);
    }
}