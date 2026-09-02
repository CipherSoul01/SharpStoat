using System;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Stoat.Client.Theme;

namespace Stoat.Client.Control.Window;

[TemplatePart("PART_TitleBar", typeof(Avalonia.Controls.Control))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
[TemplatePart("PART_MaximizeButton", typeof(Button))]
[TemplatePart("PART_CloseButton", typeof(Button))]

[TemplatePart("PART_ResizeLeft", typeof(Border))]
[TemplatePart("PART_ResizeRight", typeof(Border))]
[TemplatePart("PART_ResizeTop", typeof(Border))]
[TemplatePart("PART_ResizeBottom", typeof(Border))]

[TemplatePart("PART_ResizeTopLeft", typeof(Border))]
[TemplatePart("PART_ResizeTopRight", typeof(Border))]
[TemplatePart("PART_ResizeBottomLeft", typeof(Border))]
[TemplatePart("PART_ResizeBottomRight", typeof(Border))]

public class StoatWindow : Avalonia.Controls.Window
{
    protected override Type StyleKeyOverride
        => typeof(StoatWindow);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find("PART_TitleBar") is Avalonia.Controls.Control titleBar)
        {
            titleBar.PointerPressed += OnTitleBarPointerPressed;
            titleBar.DoubleTapped += OnTitleBarDoubleTapped;
        }

        if (e.NameScope.Find("PART_MinimizeButton") is Button minimize)
        {
            minimize.Click += (_, _) =>
                WindowState = WindowState.Minimized;
        }

        if (e.NameScope.Find("PART_MaximizeButton") is Button maximize)
        {
            maximize.Click += (_, _) =>
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
        }

        if (e.NameScope.Find("PART_CloseButton") is Button close)
        {
            close.Click += (_, _) => Close();
        }

        AddResizeGrip(e, "PART_ResizeLeft", WindowEdge.West);
        AddResizeGrip(e, "PART_ResizeRight", WindowEdge.East);
        AddResizeGrip(e, "PART_ResizeTop", WindowEdge.North);
        AddResizeGrip(e, "PART_ResizeBottom", WindowEdge.South);

        AddResizeGrip(e, "PART_ResizeTopLeft", WindowEdge.NorthWest);
        AddResizeGrip(e, "PART_ResizeTopRight", WindowEdge.NorthEast);
        AddResizeGrip(e, "PART_ResizeBottomLeft", WindowEdge.SouthWest);
        AddResizeGrip(e, "PART_ResizeBottomRight", WindowEdge.SouthEast);
    }

    private void AddResizeGrip(
        TemplateAppliedEventArgs e,
        string name,
        WindowEdge edge)
    {
        if (e.NameScope.Find(name) is not Avalonia.Controls.Control grip)
            return;

        grip.PointerPressed += (_, args) =>
        {
            if (!CanResize)
                return;

            if (WindowState == WindowState.Maximized ||
                WindowState == WindowState.FullScreen)
                return;

            var point = args.GetCurrentPoint(grip);

            if (!point.Properties.IsLeftButtonPressed)
                return;

            BeginResizeDrag(edge, args);

            args.Handled = true;
        };
    }

    private void OnTitleBarPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (!point.Properties.IsLeftButtonPressed)
            return;

        if (WindowState == WindowState.FullScreen)
            return;

        BeginMoveDrag(e);

        e.Handled = true;
    }

    private void OnTitleBarDoubleTapped(
        object? sender,
        TappedEventArgs e)
    {
        if (!CanResize || !CanMaximize)
            return;

        if (WindowState == WindowState.FullScreen)
            return;

        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}