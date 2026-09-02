using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Styling;

namespace Stoat.Client.Theme;

public static class StoatThemeVariants
{
    public static readonly ReadOnlyDictionary<string, ThemeVariant> Themes =
        new ReadOnlyDictionary<string, ThemeVariant>(new Dictionary<string, ThemeVariant>()
        {
            { "Dark", Dark },
            { "Light", Light }
        });
    
    public static readonly ThemeVariant Dark = new("StoatDark", ThemeVariant.Dark);
    
    public static readonly ThemeVariant Light = new("StoatLight", ThemeVariant.Light);
}