using Stoat.Client.ViewModel.Shared;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class RegisterViewModel : MainScreenRoutableViewModelBase
{
    public override string? UrlPathSegment => null;
    
    public RegisterViewModel(IMainScreen hostScreen) : base(hostScreen)
    {
        SetupRx();
    }
}