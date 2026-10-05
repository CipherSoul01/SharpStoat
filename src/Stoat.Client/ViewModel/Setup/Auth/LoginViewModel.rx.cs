using Lucide.Avalonia;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class LoginViewModel
{
    
    private ObservableAsPropertyHelper<LucideIconKind>? _showPasswordIconHelper;
    public LucideIconKind ShowPasswordIcon =>  _showPasswordIconHelper?.Value ?? LucideIconKind.EyeOff;

    private ObservableAsPropertyHelper<bool>? _showSecretHelper;
    public bool ShowSecret => _showSecretHelper?.Value ?? false;
    
    private string _email;
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }
    
    private string _password;
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }
    
    private bool _showPassword;
    public bool ShowPassword
    {
        get => _showPassword;
        set => this.RaiseAndSetIfChanged(ref _showPassword, value);
    }

    private int _logoClick;
    public int LogoClick
    {
        get => _logoClick;
        set => this.RaiseAndSetIfChanged(ref _logoClick, value);
    }

    protected sealed override void SetupRx()
    {
        _showPasswordIconHelper = this.WhenAnyValue(x => x.ShowPassword)
            .Select(x => x ? LucideIconKind.Eye : LucideIconKind.EyeOff)
            .ToProperty(this, x => x.ShowPasswordIcon, out _showPasswordIconHelper);
        
        _showSecretHelper = this.WhenAnyValue(x => x.LogoClick)
            .Select(x => x >= 5)
            .ToProperty(this, x => x.ShowSecret, out _showSecretHelper);
    }
}