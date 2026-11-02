using System;
using ReactiveUI;
using ReactiveUI.Primitives;
using Splat;
using Stoat.Client.ViewModel.Shared;
using Stoat.Core.Interfaces;
using Stoat.Theme.Controls.Captcha;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel : MainScreenRoutableViewModelBase, IActivatableViewModel, IDisposable
{
    private HCaptchaView? _dialog;
    private IStoatService _stoat;
    
    public LoginViewModel(IMainScreen screen) : base(screen)
    {
        _stoat = AppLocator.Current.GetService<IStoatService>()!;
        
        this.WhenActivated(disposables =>
        {
            this.DisposeWith(disposables);
        });
        
        SetupRx();
    }

    public override string? UrlPathSegment => null;
    
    public ViewModelActivator Activator { get; } = new();
    
    public void Dispose()
    {
       _dialog?.Dispose(); 
       _dialog = null;
    }
}