using System.Collections.ObjectModel;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Stoat.Theme;

public partial class ThemeStoat : Style
{
    public static readonly ObservableCollection<ThemeDefinition> Themes = new();

    public ThemeStoat()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class ThemeDefinition
{
    public string Key { get; set; }
    public ThemeVariant? Value { get; }

    public ThemeDefinition(string key, ThemeVariant? variant)
    {
        Key = key;
        Value = variant;
    }
}