using Splat;
using Stoat.Client.ViewModel.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Layout;

public sealed partial class SetupLayoutViewModel : MainScreenRoutableViewModelBase, ILayoutViewModel
{
    private readonly IStoatService _stoat;
    
    public SetupLayoutViewModel(IMainScreen hostScreen) : base(hostScreen)
    {
        _stoat = AppLocator.Current.GetService<IStoatService>()!;
        
        SetupRx();
    }

    public override string? UrlPathSegment => null;
}