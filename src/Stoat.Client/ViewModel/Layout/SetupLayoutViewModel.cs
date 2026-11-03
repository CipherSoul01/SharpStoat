using Stoat.Client.ViewModel.Layout.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Layout;

public sealed partial class SetupLayoutViewModel : SetupViewModelBase, ILayoutViewModel
{
    
    public SetupLayoutViewModel(IMainScreen hostScreen, IStoatService stoat) : base(hostScreen, stoat)
    {
        SetupRx();
    }
}