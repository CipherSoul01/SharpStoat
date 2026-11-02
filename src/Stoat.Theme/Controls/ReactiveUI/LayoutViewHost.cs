using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using Splat;
using Stoat.Theme.Attributes;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Theme.Controls.ReactiveUI;

public class LayoutViewHost :
    ContentControl,
    IActivatableView,
    IEnableLogger
{
    private static readonly ConcurrentDictionary<Type, Type?> LayoutTypes = new();
    
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

    public static readonly StyledProperty<IMainScreen> MainScreenProperty = AvaloniaProperty.Register<LayoutViewHost, IMainScreen>(
        nameof(MainScreen));

    public IMainScreen MainScreen
    {
        get => GetValue(MainScreenProperty);
        set => SetValue(MainScreenProperty, value);
    }

    private MultipleDisposable? _navigationDisposables;
    private CancellationTokenSource? _navigationCancellation;
    private object? _currentLayout;
    
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

    public IViewLocator? ViewLocator { get; set; }
    public IServiceProvider? Provider { get; set; } 

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

            var serviceProvider = Provider ??
                AppLocator.Current.GetService<IServiceProvider>() ??
                throw new NullReferenceException(typeof(IServiceProvider).FullName);

            
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
            
            var viewType = viewInstance.GetType();

            var attribute = viewType.GetCustomAttribute<LayoutHostAttribute>();
            
            if (attribute is { } layoutViewModel)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                layoutControl = GetLayoutControl(serviceProvider, viewLocator, attribute);
            }
            else
                _currentLayout = null;
            
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

    private ReactiveContentControlBase? GetLayoutControl(
        IServiceProvider provider,
        IViewLocator locator,
        LayoutHostAttribute hostAttribute)
    {
        var viewModelType = GetViewModelType(hostAttribute.LayoutControl);

        if (viewModelType is null)
            throw new NotSupportedException(
                $"Unsupported layout type: {hostAttribute.LayoutControl.FullName}");

        if (_currentLayout is IDataContextProvider context && context.DataContext?.GetType() == viewModelType)
            return null;

        var instance = ActivatorUtilities.CreateInstance(
            provider,
            viewModelType,
            MainScreen);

        var layoutInstance = locator.ResolveView(instance);

        if (layoutInstance is not ReactiveContentControlBase control)
            throw new InvalidOperationException(
                $"Layout must resolve to {nameof(ReactiveContentControlBase)}.");

        _currentLayout = layoutInstance;

        return control;
    }
    
    private static Type? GetViewModelType(Type type)
    {
        return LayoutTypes.GetOrAdd(type, static type =>
        {
            if (typeof(ReactiveObject).IsAssignableFrom(type))
                return type;

            return type.GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IViewFor<>))
                ?.GetGenericArguments()[0];
        });
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