namespace Stoat.Theme.Controls;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;


/// <summary>
/// Modal dialog (shadcn Dialog analogue): scrim overlay + centred card.
/// </summary>
public class Dialog : ContentControl
{
#pragma warning disable CS1591

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(IsOpen),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<Dialog, object?>(
            nameof(Title));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<Dialog, object?>(
            nameof(Description));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Dialog, object?>(
            nameof(Footer));

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(ShowCloseButton),
            defaultValue: true);

    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(CloseOnClickOutside),
            defaultValue: true);

    public static readonly StyledProperty<bool> CloseOnEscapeProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(CloseOnEscape),
            defaultValue: true);

    public static readonly StyledProperty<double> CardWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(
            nameof(CardWidth),
            defaultValue: double.NaN);

    public static readonly StyledProperty<double> CardMinWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(
            nameof(CardMinWidth),
            defaultValue: 320d);

    public static readonly StyledProperty<double> CardMaxWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(
            nameof(CardMaxWidth),
            defaultValue: 480d);

    public static readonly StyledProperty<double> CardHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(
            nameof(CardHeight),
            defaultValue: double.NaN);

    public static readonly StyledProperty<double> CardMaxHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(
            nameof(CardMaxHeight),
            defaultValue: double.PositiveInfinity);

    public static readonly StyledProperty<bool> OverlayProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(Overlay),
            defaultValue: true);

#pragma warning restore CS1591

    /// <summary>
    /// Whether the dialog is open.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Dialog title.
    /// </summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Description below the title.
    /// </summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Footer content.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Whether to show the close button.
    /// </summary>
    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    /// <summary>
    /// Whether clicking outside the card closes the dialog.
    /// </summary>
    public bool CloseOnClickOutside
    {
        get => GetValue(CloseOnClickOutsideProperty);
        set => SetValue(CloseOnClickOutsideProperty, value);
    }

    /// <summary>
    /// Whether pressing Escape closes the dialog.
    /// </summary>
    public bool CloseOnEscape
    {
        get => GetValue(CloseOnEscapeProperty);
        set => SetValue(CloseOnEscapeProperty, value);
    }

    /// <summary>
    /// Explicit card width.
    /// </summary>
    public double CardWidth
    {
        get => GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    /// <summary>
    /// Minimum card width.
    /// </summary>
    public double CardMinWidth
    {
        get => GetValue(CardMinWidthProperty);
        set => SetValue(CardMinWidthProperty, value);
    }

    /// <summary>
    /// Maximum card width.
    /// </summary>
    public double CardMaxWidth
    {
        get => GetValue(CardMaxWidthProperty);
        set => SetValue(CardMaxWidthProperty, value);
    }

    /// <summary>
    /// Explicit card height.
    /// </summary>
    public double CardHeight
    {
        get => GetValue(CardHeightProperty);
        set => SetValue(CardHeightProperty, value);
    }

    /// <summary>
    /// Maximum card height.
    /// </summary>
    public double CardMaxHeight
    {
        get => GetValue(CardMaxHeightProperty);
        set => SetValue(CardMaxHeightProperty, value);
    }

    /// <summary>
    /// Whether the dimming scrim is shown.
    /// </summary>
    public bool Overlay
    {
        get => GetValue(OverlayProperty);
        set => SetValue(OverlayProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Dialog);

    private static readonly IBrush ScrimBrush =
        new SolidColorBrush(Color.Parse("#99000000"));

    private Border? _scrim;
    private Panel? _root;
    private Button? _closeButton;

    static Dialog()
    {
        IsOpenProperty.Changed.AddClassHandler<Dialog>((dialog, _) =>
        {
            dialog.PseudoClasses.Set(":open", dialog.IsOpen);
            dialog.ApplyOpenState();
        });

        OverlayProperty.Changed.AddClassHandler<Dialog>((dialog, _) =>
        {
            dialog.ApplyOverlay();
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _root = e.NameScope.Find<Panel>("PART_Root");
        _scrim = e.NameScope.Find<Border>("PART_Scrim");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");

        if (_scrim is not null)
        {
            _scrim.AddHandler(
                PointerPressedEvent,
                OnScrimPressed,
                RoutingStrategies.Tunnel);

            _scrim.AddHandler(
                KeyDownEvent,
                OnScrimKeyDown);
        }

        if (_closeButton is not null)
        {
            _closeButton.AddHandler(
                Button.ClickEvent,
                OnCloseButtonClick);
        }

        PseudoClasses.Set(":open", IsOpen);

        ApplyOverlay();
        ApplyOpenState();
    }

    protected override void OnDetachedFromVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        if (_scrim is not null)
        {
            _scrim.RemoveHandler(
                PointerPressedEvent,
                OnScrimPressed);

            _scrim.RemoveHandler(
                KeyDownEvent,
                OnScrimKeyDown);
        }

        if (_closeButton is not null)
        {
            _closeButton.RemoveHandler(
                Button.ClickEvent,
                OnCloseButtonClick);
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void ApplyOverlay()
    {
        if (_scrim is null)
            return;

        _scrim.Background = Overlay
            ? ScrimBrush
            : Brushes.Transparent;
    }

    private void ApplyOpenState()
    {
        if (_root is null || _scrim is null)
            return;

        var open = IsOpen;

        _root.IsVisible = open;

        _scrim.Opacity = open ? 1 : 0;
        _scrim.IsHitTestVisible = open;
    }

    private void OnCloseButtonClick(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
        e.Handled = true;
    }

    private void OnScrimKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;

        if (!IsOpen || !CloseOnEscape)
            return;

        Close();
        e.Handled = true;
    }

    private void OnScrimPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (!CloseOnClickOutside)
            return;

        if (ReferenceEquals(e.Source, sender))
        {
            Close();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Opens the dialog.
    /// </summary>
    public void Open()
    {
        SetCurrentValue(IsOpenProperty, true);
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public void Close()
    {
        SetCurrentValue(IsOpenProperty, false);
    }
}