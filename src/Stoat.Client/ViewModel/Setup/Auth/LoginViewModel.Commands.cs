using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel
{
    [ReactiveCommand]
    private void Previous()
        => HostScreen.Router.NavigateBack.Execute();
}