using System;
using System.Reactive.Disposables.Fluent;
using ReactiveUI;
using ReactiveUI.Avalonia;
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