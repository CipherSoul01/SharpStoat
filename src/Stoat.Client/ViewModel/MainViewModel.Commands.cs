using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;
using Stoat.Client.ViewModel.Home;
using Stoat.Client.ViewModel.Setup;

namespace Stoat.Client.ViewModel;

public partial class MainViewModel
{
    [ReactiveCommand]
    private Task SetupAsync()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Router.Navigate.Execute(new SetupViewModel(this));
        });
        
        return Task.CompletedTask;
    }
}