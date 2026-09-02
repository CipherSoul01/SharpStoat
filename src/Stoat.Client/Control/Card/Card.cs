using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Stoat.Client.Control.Card;

public class Card : TemplatedControl
{
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<Card, object?>(
        nameof(Header));
    
    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<Card, object?>(
        nameof(Content));

    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Card, object?>(
        nameof(Footer));

    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<Card, double>(
        nameof(Spacing));
    
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
    
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}