using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using Stoat.Client.ViewModel;

namespace Stoat.Client.View;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        ViewModel = new MainViewModel();
        
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel!.SetupCommand.Execute()
                .Subscribe()
                .DisposeWith(disposables);
        });
    }
}