using Avalonia.Controls;
using ReactiveUI.Avalonia;
using Stoat.Client.View.Layout;
using Stoat.Client.ViewModel.Layout;
using Stoat.Client.ViewModel.Setup.Auth;
using Stoat.Theme.Attributes;

namespace Stoat.Client.View.Setup.Auth;

[LayoutHost(typeof(SetupLayout))]
public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();
    }
}