using ReactiveUI;
using Stoat.Client.Interfaces;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class AuthViewModel : ViewModelBase, IMultipleRoutableViewModel
{
    public string? UrlPathSegment { get; } = "";
    public IScreen HostScreen { get; }
    
    public IScreen SecondHostScreen { get; }
    
    public AuthViewModel(IScreen hostScreen, IScreen secondHostScreen)
    {
        HostScreen = hostScreen;
        SecondHostScreen = secondHostScreen;
    }

}