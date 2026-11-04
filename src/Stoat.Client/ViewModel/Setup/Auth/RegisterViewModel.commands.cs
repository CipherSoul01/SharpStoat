using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ReactiveUI.Primitives.Signals;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class RegisterViewModel
{
    [ReactiveCommand]
    private async Task Previous()
        => await HostScreen.Router.NavigateBack.Execute();

    [ReactiveCommand]
    private void InvertShowPassword()
        => ShowPassword = !ShowPassword;

    [ReactiveCommand]
    private void GeneratePassword()
    {
        var password = CreatePassword();
        
        ShowPassword = true;
        
        Password = password;
        ConfirmPassword = password;
    }
    
    [ReactiveCommand]
    private void InvertShowConfirmPassword()
        => ShowConfirmPassword = !ShowConfirmPassword;
    
    private static string CreatePassword(int length = 15)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        const string special = "!@#$%^&*()-_=+";

        const string chars = lowercase + uppercase + numbers + special;

        if (length < 8)
            throw new ArgumentOutOfRangeException(nameof(length), "Password must have at least 8 characters.");

        var password = new char[length];

        password[0] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
        password[1] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
        password[2] = numbers[RandomNumberGenerator.GetInt32(numbers.Length)];
        password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (var i = 4; i < length; i++)
            password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        RandomNumberGenerator.Shuffle(password);

        return new string(password);
    }
}