using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using Splat;
using Stoat.Client.ViewModel;
using Stoat.Core.Interfaces;

namespace Stoat.Client.View;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        ViewModel = new MainViewModel(
            AppLocator.Current.GetService<IStoatService>()!);
        
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel!.SetupCommand.Execute()
                .Subscribe()
                .DisposeWith(disposables);
        });
    }
}