using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Stoat.Theme.Controls.ReactiveUI;

public class LayoutViewHost : ContentControl, IActivatableView
{
    public static readonly StyledProperty<LayoutState?> LayoutProperty =
        AvaloniaProperty.Register<LayoutViewHost, LayoutState?>(
            nameof(Layout));

    public static readonly StyledProperty<object?> DefaultContentProperty =
        AvaloniaProperty.Register<LayoutViewHost, object?>(
            nameof(DefaultContent));

    public IViewLocator? ViewLocator { get; set; }

    public LayoutViewHost() =>
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.Layout)
                .Where(x => x is not null)
                .SelectMany(layout =>
                    layout!.WhenAnyValue(x => x.Current))
                .Subscribe(UpdateLayout)
                .DisposeWith(disposables);
        });

    public LayoutState? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public object? DefaultContent
    {
        get => GetValue(DefaultContentProperty);
        set => SetValue(DefaultContentProperty, value);
    }

    private void UpdateLayout(object? viewModel)
    {
        var defaultContent = DefaultContent;

        if (Layout == null || viewModel == null)
        {
            Content = defaultContent;
            return;
        }

        var viewLocator = ViewLocator ?? global::ReactiveUI.ViewLocator.Current;
        var viewInstance = viewLocator.ResolveView(viewModel);

        if (viewInstance == null)
        {
            Content = defaultContent;
            return;
        }

        viewInstance.ViewModel = viewModel;

        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModel;
        }

        Content = viewInstance;
    }
}