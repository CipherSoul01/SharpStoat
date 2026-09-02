namespace Stoat.Client.I18n;

public static class Lang
{
    public static readonly string Version = "beta 1.0";
    
    private static readonly string Action = "Localization.Actions";
    
    public static readonly string Confirm = $"{Action}.Confirm";
    public static readonly string Cancel = $"{Action}.Cancel";
    public static readonly string Next = $"{Action}.Next";
    public static readonly string Previous = $"{Action}.Previous";

    private static readonly string Footer = $"Localization.Footer";
    
    public static readonly string About = $"{Footer}.About";
    public static readonly string TermsOfService = $"{Footer}.TermsOfService";
    public static readonly string PrivacyPolicy = $"{Footer}.PrivacyPolicy";
    
    private static readonly string SetupAuth = $"Localization.Setup.Auth";
    
    public static readonly string Title = $"{SetupAuth}.Title";
    public static readonly string Description = $"{SetupAuth}.Description";
    public static readonly string SignIn= $"{SetupAuth}.SignIn";
    public static readonly string SignUp = $"{SetupAuth}.SignUp";

    public static readonly string Welcome = $"{SetupAuth}.Welcome";
    public static readonly string Prompt = $"{SetupAuth}.Prompt";
    public static readonly string Email = $"{SetupAuth}.Email";
    public static readonly string Password = $"{SetupAuth}.Password";
    public static readonly string ForgotPassword = $"{SetupAuth}.ForgotPassword";
    public static readonly string Back = $"{SetupAuth}.Back";
    public static readonly string Login = $"{SetupAuth}.Login";
    public static readonly string RememberMe = $"{SetupAuth}.RememberMe";
}