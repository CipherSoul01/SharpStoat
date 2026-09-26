using Stoat.Client.ViewModel.Shared;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class AuthViewModel : MainScreenRoutableViewModelBase
{
    public override string? UrlPathSegment => null;
    
    public AuthViewModel(IMainScreen screen) : base(screen)
    {
        
    }

}