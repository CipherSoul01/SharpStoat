using System.Threading.Tasks;
using ReactiveUI.Primitives.Signals;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel
{
    [ReactiveCommand]
    private async Task Previous()
        => await HostScreen.Router.NavigateBack.Execute();
}