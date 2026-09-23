using ReactiveUI;
using Stoat.Theme.Controls.ReactiveUI;

namespace Stoat.Theme.Interfaces.Routing;

public interface IMainScreen : IScreen
{
    public LayoutState Layout { get; } 
}

public interface IMainScreenRoutableViewModel : IRoutableViewModel
{
    new IMainScreen HostScreen { get; }
}