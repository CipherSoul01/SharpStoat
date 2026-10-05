using ReactiveUI;
using Stoat.Client.ViewModel.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Controls.ReactiveUI;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel;

public sealed partial class MainViewModel : ViewModelBase, IMainScreen
{
    private readonly IStoatService _stoat;
    
    public RoutingState Router { get; } = new();
    public LayoutState Layout { get; } = new();

    public MainViewModel(IStoatService stoat)
    {
        _stoat = stoat;
    }
}