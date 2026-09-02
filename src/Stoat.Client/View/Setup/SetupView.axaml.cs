using System;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Stoat.Client.ViewModel.Setup;

namespace Stoat.Client.View.Setup;

public partial class SetupView : ReactiveUserControl<SetupViewModel>
{
    public SetupView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            ViewModel = new SetupViewModel(null);

        this.WhenActivated(disposables =>
        {
            ViewModel!.SetupCommand
                .Execute()
                .Subscribe()
                .DisposeWith(disposables);
        });
    }
}