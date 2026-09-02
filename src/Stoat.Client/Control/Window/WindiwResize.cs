using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Stoat.Client.Control.Window;

public static class WindowResize
{
    public static void AddResizeGrip(Avalonia.Controls.Window window, Panel root)
    {
        AddGrip(window, root, WindowEdge.West, "ResizeLeft");
        AddGrip(window, root, WindowEdge.East, "ResizeRight");
        AddGrip(window, root, WindowEdge.North, "ResizeTop");
        AddGrip(window, root, WindowEdge.South, "ResizeBottom");

        AddGrip(window, root, WindowEdge.NorthWest, "ResizeTopLeft");
        AddGrip(window, root, WindowEdge.NorthEast, "ResizeTopRight");
        AddGrip(window, root, WindowEdge.SouthWest, "ResizeBottomLeft");
        AddGrip(window, root, WindowEdge.SouthEast, "ResizeBottomRight");
    }

    private static void AddGrip(
        Avalonia.Controls.Window window,
        Panel root,
        WindowEdge edge,
        string name)
    {
        var grip = new Border
        {
            Name = name,
            Background = Avalonia.Media.Brushes.Transparent,
            Width = edge is WindowEdge.West or WindowEdge.East
                or WindowEdge.NorthWest or WindowEdge.NorthEast
                or WindowEdge.SouthWest or WindowEdge.SouthEast
                ? 6
                : double.NaN,

            Height = edge is WindowEdge.North or WindowEdge.South
                ? 6
                : double.NaN,

            HorizontalAlignment = GetHorizontalAlignment(edge),
            VerticalAlignment = GetVerticalAlignment(edge),
            ZIndex = 1000
        };

        grip.PointerPressed += (_, e) =>
        {
            if (e.Handled)
                return;

            if (e.GetCurrentPoint(window).Properties.PointerUpdateKind
                != PointerUpdateKind.LeftButtonPressed)
                return;

            window.BeginResizeDrag(edge, e);

            e.Handled = true;
        };

        root.Children.Add(grip);
    }

    private static Avalonia.Layout.HorizontalAlignment GetHorizontalAlignment(
        WindowEdge edge)
    {
        return edge switch
        {
            WindowEdge.West or
            WindowEdge.NorthWest or
            WindowEdge.SouthWest
                => Avalonia.Layout.HorizontalAlignment.Left,

            WindowEdge.East or
            WindowEdge.NorthEast or
            WindowEdge.SouthEast
                => Avalonia.Layout.HorizontalAlignment.Right,

            _ => Avalonia.Layout.HorizontalAlignment.Stretch
        };
    }

    private static Avalonia.Layout.VerticalAlignment GetVerticalAlignment(
        WindowEdge edge)
    {
        return edge switch
        {
            WindowEdge.North or
            WindowEdge.NorthWest or
            WindowEdge.NorthEast
                => Avalonia.Layout.VerticalAlignment.Top,

            WindowEdge.South or
            WindowEdge.SouthWest or
            WindowEdge.SouthEast
                => Avalonia.Layout.VerticalAlignment.Bottom,

            _ => Avalonia.Layout.VerticalAlignment.Stretch
        };
    }
}