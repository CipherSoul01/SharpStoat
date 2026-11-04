using Lucide.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class RegisterViewModel
{
    private ObservableAsPropertyHelper<LucideIconKind>? _showPasswordIconKindHelper;
    private ObservableAsPropertyHelper<LucideIconKind>? _showConfirmPasswordIconKindHelper;
    
    [Reactive] private string _email;
    
    [Reactive] private bool _showPassword;
    [Reactive] private bool _showConfirmPassword;
    
    [Reactive] private string _password;
    [Reactive] private string _confirmPassword;

    public LucideIconKind ShowPasswordIconKind => _showPasswordIconKindHelper?.Value ?? LucideIconKind.EyeOff;
    public LucideIconKind ShowConfirmPasswordIconKind => _showConfirmPasswordIconKindHelper?.Value ?? LucideIconKind.EyeOff; 
    
    protected sealed override void SetupRx()
    {
        _showPasswordIconKindHelper = this.WhenAnyValue(x => x.ShowPassword)
            .Select(x => x ? LucideIconKind.EyeOff : LucideIconKind.Eye)
            .ToProperty(this, x => x.ShowPasswordIconKind, out _showPasswordIconKindHelper);
        
        _showConfirmPasswordIconKindHelper = this.WhenAnyValue(x => x.ShowConfirmPassword)
            .Select(x => x ? LucideIconKind.EyeOff : LucideIconKind.Eye)
            .ToProperty(this, x => x.ShowConfirmPasswordIconKind, out _showConfirmPasswordIconKindHelper);
    }
}