using Splat;
using Stoat.Client.ViewModel.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class SetupViewModel : MainScreenRoutableViewModelBase
{
    public override string? UrlPathSegment => null;
    
    private readonly IStoatService _stoat;
    
    public SetupViewModel(IMainScreen screen) : base(screen)
    {
        _stoat = AppLocator.Current.GetService<IStoatService>()!;
    }

}