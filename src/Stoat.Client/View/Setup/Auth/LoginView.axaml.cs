using Avalonia.Controls;
using ReactiveUI.Avalonia;
using Stoat.Client.ViewModel.Setup.Auth;

namespace Stoat.Client.View.Setup.Auth;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();
    }
}