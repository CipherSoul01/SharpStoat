using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace Stoat.Theme.Controls.ReactiveUI;

public class LayoutViewHost : TransitioningContentControl, IActivatableView, IEnableLogger
{
    private static void Throw(Exception error) =>
        ExceptionDispatchInfo.Capture(error).Throw();

    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<LayoutViewHost, RoutingState?>(
            nameof(Router));

    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<LayoutViewHost, string?>(
            nameof(ViewContract));

    public static readonly StyledProperty<object?> DefaultContentProperty =
        ViewModelViewHost.DefaultContentProperty.AddOwner<LayoutViewHost>();

    public static readonly StyledProperty<LayoutState?> LayoutProperty =
        AvaloniaProperty.Register<LayoutViewHost, LayoutState?>(
            nameof(Layout));

    private MultipleDisposable? _navigationDisposables;
    private ReactiveContentControlBase? _currentLayout = null;

    public RoutingState? Router
    {
        get => GetValue(RouterProperty);
        set => SetValue(RouterProperty, value);
    }

    public string? ViewContract
    {
        get => GetValue(ViewContractProperty);
        set => SetValue(ViewContractProperty, value);
    }

    public object? DefaultContent
    {
        get => GetValue(DefaultContentProperty);
        set => SetValue(DefaultContentProperty, value);
    }

    public LayoutState? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public IViewLocator? ViewLocator { get; set; }

    protected override Type StyleKeyOverride =>
        typeof(TransitioningContentControl);

    internal void NavigateToViewModel(
        object? viewModel,
        string? contract)
    {
        if (Router is null)
        {
            this.Log().Warn(
                "Router property is null. Falling back to default content.");

            Content = DefaultContent;
            return;
        }

        if (viewModel is null)
        {
            this.Log().Info(
                "ViewModel is null. Falling back to default content.");

            Content = DefaultContent;
            return;
        }

        var viewLocator =
            ViewLocator ?? global::ReactiveUI.ViewLocator.Current;

        var viewInstance = viewLocator.ResolveView(
            viewModel,
            contract);

        if (viewInstance is null)
        {
            LogMissingView(viewModel, contract);
            Content = DefaultContent;
            return;
        }

        var resolvedMessage = contract is null
            ? $"Ready to show {viewInstance} with autowired {viewModel}."
            : $"Ready to show {viewInstance} with autowired {viewModel} and contract '{contract}'.";

        this.Log().Info(resolvedMessage);

        viewInstance.ViewModel = viewModel;

        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModel;
        }

        if (Layout?.Current is { } layoutViewModel)
        {
            var layoutInstance = viewLocator.ResolveView(layoutViewModel);

            if (layoutInstance is ReactiveContentControlBase layoutControl)
            {
                layoutControl.Content = viewInstance;
                
                Content = layoutControl;
                
                return;
            }
        }

        Content = viewInstance;
    }

    protected override void OnAttachedToVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _navigationDisposables ??= CreateNavigationDisposables();
    }

    protected override void OnDetachedFromVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        DisposeNavigationDisposables();

        base.OnDetachedFromVisualTree(e);
    }

    private void DisposeNavigationDisposables()
    {
        var disposables = _navigationDisposables;

        _navigationDisposables = null;

        disposables?.Dispose();
    }

    private static IObservable<object?> CreateRouterViewModelObservable(
        RoutingState router) =>
        router.CurrentViewModel
            .Select(static viewModel => (object?)viewModel);

    private MultipleDisposable CreateNavigationDisposables()
    {
        var disposables = new MultipleDisposable();

        var routerChanges =
            this.GetObservable(RouterProperty);

        var viewContract =
            this.GetObservable(ViewContractProperty);

        var viewModels = routerChanges
            .Select(static router =>
                router is null
                    ? Signal.Return<object?>(null)
                    : CreateRouterViewModelObservable(router))
            .Switch();

        var navigation = viewModels
            .CombineLatest(
                viewContract,
                static (viewModel, contract) =>
                    new NavigationTarget(viewModel, contract));

        var subscription = LinqExtensions.SubscribeSafe(
            navigation,
            target => NavigateToViewModel(
                target.ViewModel,
                target.Contract),
            Throw);

        disposables.Add(subscription);

        return disposables;
    }

    private void LogMissingView(
        object viewModel,
        string? contract)
    {
        if (contract is null)
        {
            this.Log().Warn(
                $"Couldn't find view for '{viewModel}'. " +
                "Is it registered? Falling back to default content.");

            return;
        }

        this.Log().Warn(
            $"Couldn't find view with contract '{contract}' for '{viewModel}'. " +
            "Is it registered? Falling back to default content.");
    }

    private readonly record struct NavigationTarget(
        object? ViewModel,
        string? Contract);
}