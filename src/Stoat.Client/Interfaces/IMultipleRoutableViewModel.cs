using ReactiveUI;

namespace Stoat.Client.Interfaces;

public interface IMultipleRoutableViewModel : IRoutableViewModel
{
    IScreen SecondHostScreen { get; }
}