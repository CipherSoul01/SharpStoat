using System;
using System.Threading.Tasks;
using ReactiveUI.Primitives.Signals;
using ReactiveUI.SourceGenerators;
using Stoat.Client.ViewModel.Layout;
using Stoat.Client.ViewModel.Setup.Auth;

namespace Stoat.Client.ViewModel;

public partial class MainViewModel
{
    [ReactiveCommand]
    private async Task SetupAsync()
    {
        
        await Layout.SetLayoutCommand
            .Execute(new SetupLayoutViewModel(this));
        
        await Router.Navigate
            .Execute(new AuthViewModel(this));
    }
}