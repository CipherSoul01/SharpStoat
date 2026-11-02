using Stoat.Client.ViewModel.Layout;
using Stoat.Theme.Controls.ReactiveUI;

namespace Stoat.Client.View.Layout;

public partial class AuthLayout : ReactiveContentControl<AuthLayoutViewModel>
{
    public AuthLayout()
    {
        InitializeComponent();
    }
}