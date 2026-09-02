using ReactiveUI;
using Stoat.Client.Interfaces;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel : ViewModelBase, IMultipleRoutableViewModel
{
    public string? UrlPathSegment { get; } = null;
    public IScreen HostScreen { get; set; }
    public IScreen SecondHostScreen { get; set; }

    public LoginViewModel(IScreen hostScreen, IScreen secondHostScreen)
    {
        HostScreen = hostScreen;
        SecondHostScreen = secondHostScreen;
    }
}