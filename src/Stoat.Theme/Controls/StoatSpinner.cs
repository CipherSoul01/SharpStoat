using Avalonia;
using Avalonia.Controls.Primitives;

namespace Stoat.Theme.Controls;

public class StoatSpinner : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(StoatSpinner);

    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<StoatSpinner, double>(
        nameof(Size));

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}