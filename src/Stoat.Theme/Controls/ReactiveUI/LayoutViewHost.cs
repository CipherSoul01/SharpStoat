using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace Stoat.Theme.Controls.ReactiveUI;

public class LayoutViewHost :
    ContentControl,
    IActivatableView,
    IEnableLogger
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

    private CancellationTokenSource? _navigationCancellation;

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
        typeof(ContentControl);

    private static IObservable<object?> CreateRouterViewModelObservable(
        RoutingState router) =>
        router.CurrentViewModel
            .Select(static viewModel => (object?)viewModel);

    protected override void OnAttachedToVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _navigationDisposables ??=
            CreateNavigationDisposables();
    }

    protected override void OnDetachedFromVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        DisposeNavigationDisposables();

        base.OnDetachedFromVisualTree(e);
    }

    private void DisposeNavigationDisposables()
    {
        _navigationDisposables?.Dispose();
        _navigationDisposables = null;

        CancelNavigation();
    }

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
                    new NavigationTarget(
                        viewModel,
                        contract));

        var subscription = LinqExtensions.SubscribeSafe(
            navigation,
            NavigateToViewModel,
            Throw);

        disposables.Add(subscription);

        return disposables;
    }

    private void NavigateToViewModel(
        NavigationTarget target)
    {
        CancelNavigation();

        var cancellation =
            new CancellationTokenSource();

        _navigationCancellation = cancellation;

        _ = NavigateToViewModelAsync(
            target.ViewModel,
            target.Contract,
            cancellation.Token);
    }

    private async Task NavigateToViewModelAsync(
        object? viewModel,
        string? contract,
        CancellationToken cancellationToken)
    {
        try
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

            cancellationToken.ThrowIfCancellationRequested();

            var viewLocator =
                ViewLocator ??
                global::ReactiveUI.ViewLocator.Current;

            
            var viewInstance =
                viewLocator.ResolveView(
                    viewModel,
                    contract);

            cancellationToken.ThrowIfCancellationRequested();

            if (viewInstance is null)
            {
                LogMissingView(
                    viewModel,
                    contract);

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

            cancellationToken.ThrowIfCancellationRequested();

            ReactiveContentControlBase? layoutControl = null;

            if (Layout?.Current is { } layoutViewModel)
            {
                var layoutInstance =
                    viewLocator.ResolveView(layoutViewModel);

                cancellationToken.ThrowIfCancellationRequested();

                if (layoutInstance is ReactiveContentControlBase control)
                {
                    layoutControl = control;
                }
            }

            
            await Dispatcher.Resume(
                DispatcherPriority.Background);

            cancellationToken.ThrowIfCancellationRequested();
            
            if (layoutControl is not null)
            {
                layoutControl.Content = viewInstance;

                cancellationToken.ThrowIfCancellationRequested();

                Content = layoutControl;
            }
            else
            {
                Content = viewInstance;
            }
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception error)
        {
            Throw(error);
        }
    }

    private void CancelNavigation()
    {
        var cancellation = _navigationCancellation;

        _navigationCancellation = null;

        if (cancellation is null)
            return;

        cancellation.Cancel();
        cancellation.Dispose();
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