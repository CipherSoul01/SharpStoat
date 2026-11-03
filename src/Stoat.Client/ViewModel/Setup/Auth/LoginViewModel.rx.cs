using Lucide.Avalonia;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel
{
    
    private ObservableAsPropertyHelper<LucideIconKind>? _showPasswordIconHelper;
    public LucideIconKind ShowPasswordIcon =>  _showPasswordIconHelper?.Value ?? LucideIconKind.EyeOff;

    private ObservableAsPropertyHelper<bool>? _showSecretHelper;
    public bool ShowSecret => _showSecretHelper?.Value ?? false;
    
    [Reactive]
    private bool _isRegister;
    
    [Reactive]
    private string _email;
    
    [Reactive]
    private string _password;
    
    [Reactive]
    private bool _showPassword;

    [Reactive]
    private int _logoClick;

    protected sealed override void SetupRx()
    {
        _showPasswordIconHelper = this.WhenAnyValue(x => x.ShowPassword)
            .Select(x => x ? LucideIconKind.EyeOff : LucideIconKind.Eye)
            .ToProperty(this, x => x.ShowPasswordIcon, out _showPasswordIconHelper);
        
        _showSecretHelper = this.WhenAnyValue(x => x.LogoClick)
            .Select(x => x >= 5)
            .ToProperty(this, x => x.ShowSecret, out _showSecretHelper);
    }
}