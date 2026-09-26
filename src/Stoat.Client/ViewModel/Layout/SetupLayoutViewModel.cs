using Stoat.Client.ViewModel.Shared;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Layout;

public sealed partial class SetupLayoutViewModel : MainScreenRoutableViewModelBase, ILayoutViewModel
{
    public SetupLayoutViewModel(IMainScreen hostScreen) : base(hostScreen)
    {
        SetupRx();
    }

    public override string? UrlPathSegment => null;
}