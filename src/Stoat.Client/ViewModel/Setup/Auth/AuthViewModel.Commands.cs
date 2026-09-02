using System;
using System.Diagnostics;
using ReactiveUI.SourceGenerators;

namespace Stoat.Client.ViewModel.Setup.Auth;

public partial class AuthViewModel
{
    [ReactiveCommand]
    private void OpenLoginPage()
    {
        HostScreen.Router.Navigate.Execute(new LoginViewModel(HostScreen, SecondHostScreen));
    }

    [ReactiveCommand]
    private void OpenRegisterPage()
    {
        var url = "https://stoat.chat/login/create";
        
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = url,
                UseShellExecute = false
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments = url,
                UseShellExecute = false
            });
        }
    }
}