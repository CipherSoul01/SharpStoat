using ReactiveUI;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Shared;

public abstract class MainScreenRoutableViewModelBase : ViewModelBase, IMainScreenRoutableViewModel 
{
    public abstract string? UrlPathSegment { get; }
    
    public IMainScreen HostScreen { get; private set; }
    IScreen IRoutableViewModel.HostScreen => HostScreen;

    public MainScreenRoutableViewModelBase(IMainScreen hostScreen)
    {
        HostScreen = hostScreen;
    }
}