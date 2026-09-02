using System.Reactive.Linq;
using Lucide.Avalonia;
using ReactiveUI;

namespace Stoat.Client.ViewModel.Setup;

public partial class SetupViewModel
{
    private ObservableAsPropertyHelper<LucideIconKind>? _lucideIconKindHelper;

    public LucideIconKind Kind 
        => _lucideIconKindHelper?.Value ?? LucideIconKind.Moon;

    protected override void SetupRx()
    {
        _lucideIconKindHelper = this.WhenAnyValue(x => x.IsDarkTheme)
            .Select(x => x ? LucideIconKind.Moon : LucideIconKind.Sun)
            .ToProperty(this, x => x.Kind);
    }
}