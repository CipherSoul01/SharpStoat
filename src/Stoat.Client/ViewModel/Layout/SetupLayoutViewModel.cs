using ReactiveUI;
using ReactiveUI.Primitives;
using Stoat.Client.ViewModel.Layout.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Layout;

public sealed partial class SetupLayoutViewModel : SetupViewModelBase, ILayoutViewModel, IActivatableViewModel
{
    
    public SetupLayoutViewModel(IMainScreen hostScreen, IStoatService stoat) : base(hostScreen, stoat)
    {
        this.WhenActivated(disposables =>
        {
            SetupCommand.Execute()
                .SubscribePrimitives()
                .DisposeWith(disposables);
        });
        
        SetupRx();
    }

    public ViewModelActivator Activator { get; } = new();
}