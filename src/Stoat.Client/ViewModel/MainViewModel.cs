using ReactiveUI;
using Stoat.Client.ViewModel.Shared;
using Stoat.Theme.Controls.ReactiveUI;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel;

public sealed partial class MainViewModel : ViewModelBase, IMainScreen 
{
    public RoutingState Router { get; } = new();
    public LayoutState Layout { get; } = new();
}