using Stoat.Client.ViewModel.Shared;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel : MainScreenRoutableViewModelBase 
{
    public LoginViewModel(IMainScreen screen) : base(screen)
    {
        
    }

    public override string? UrlPathSegment => null;
}