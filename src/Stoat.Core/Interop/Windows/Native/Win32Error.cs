using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Stoat.Core.Interop.Windows.Native;

internal static class Win32Error
{
    public const int Success = 0;
    public const int NotFound = 1168;

    public static int GetLastError(bool success)
        => success ? Success : Marshal.GetLastWin32Error();

    public static void ThrowIfError(
        int error,
        string defaultErrorMessage = "Unknown error.")
    {
        if (error == Success)
            return;

        throw new InteropException(
            defaultErrorMessage,
            new Win32Exception(error));
    }

    public static void ThrowIfError(
        bool succeeded,
        string defaultErrorMessage = "Unknown error.")
    {
        ThrowIfError(GetLastError(succeeded), defaultErrorMessage);
    }
}
