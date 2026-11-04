using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using Stoat.Client.View.Layout;
using Stoat.Client.ViewModel.Setup.Auth;
using Stoat.Theme.Attributes;

namespace Stoat.Client.View.Setup.Auth;

[LayoutHost(typeof(SetupLayout))]
public partial class RegisterView : ReactiveUserControl<RegisterViewModel>
{
    public RegisterView()
    {
        InitializeComponent();
    }
}