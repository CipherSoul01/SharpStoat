using ReactiveUI;
using Stoat.Client.ViewModel.Shared;

namespace Stoat.Client.ViewModel.Home;

public class HomeViewModel : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment => null;
    public IScreen HostScreen { get; }
    
    public HomeViewModel(IScreen hostScreen)
        => HostScreen = hostScreen;
}