using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using Stoat.Client.ViewModel.Layout;
using Stoat.Theme.Controls.ReactiveUI;

namespace Stoat.Client.View.Layout;

public partial class SetupLayout : ReactiveContentControl<SetupLayoutViewModel> 
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