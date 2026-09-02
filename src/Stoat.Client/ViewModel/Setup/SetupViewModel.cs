using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup;

public partial class SetupViewModel : ViewModelBase, IRoutableViewModel, IScreen
{
    [Reactive]
    private bool _isDarkTheme = true;
    
    public string? UrlPathSegment { get; } = "";
    public IScreen HostScreen { get; }

    public SetupViewModel(IScreen hostScreen) : base()
    {
        HostScreen = hostScreen;
    }

    public RoutingState Router { get; } = new RoutingState();
}