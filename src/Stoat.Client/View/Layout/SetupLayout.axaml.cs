using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using Stoat.Client.ViewModel.Layout;

namespace Stoat.Client.View.Layout;

public partial class SetupLayout : ReactiveUserControl<SetupLayoutViewModel> 
{
    public SetupLayout()
    {
        InitializeComponent();

        this.WhenActivated(disponsables =>
        {
            ViewModel!.SetupCommand
                .Execute()
                .Subscribe()
                .DisposeWith(disponsables);
        });
    }
}