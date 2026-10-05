using Splat;
using Stoat.Client.ViewModel.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class AuthViewModel : MainScreenRoutableViewModelBase
{
    public override string? UrlPathSegment => null;
    
    private readonly IStoatService _stoat;
    
    public AuthViewModel(IMainScreen screen) : base(screen)
    {
        _stoat = AppLocator.Current.GetService<IStoatService>()!;
    }

}