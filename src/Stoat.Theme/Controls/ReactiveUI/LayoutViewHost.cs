using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    TransitioningContentControl,
    IActivatableView,
    IEnableLogger
{
    private static readonly ConcurrentDictionary<Type, Type?> LayoutTypes = new();
    
    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<LayoutViewHost, RoutingState?>(nameof(Router));

    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<LayoutViewHost, string?>(nameof(ViewContract));

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
    private object? _currentLayout;

    public object? CurrentLayout
    {
        get => _currentLayout;
        set => _currentLayout = value;
    }

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

    public IServiceProvider? Provider { get; set; } 
    
    public IViewLocator? ViewLocator { get; set; }

    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    internal void NavigateToViewModel(object? viewModel, string? contract)
    {
        if (Router is null)
        {
            this.Log().Warn("Router property is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        if (viewModel is null)
        {
            this.Log().Info("ViewModel is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }
        
        Content = ViewHostNavigation.ResolveView(this, viewModel, contract, ViewLocator) ?? DefaultContent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void DisposeNavigationDisposables() => ViewHostNavigation.Release(ref _navigationDisposables);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) =>
        _navigationDisposables ??= CreateNavigationDisposables(e);

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposeNavigationDisposables();
    }

    private static object? GetCurrentViewModel(RoutingState router) =>
        router.NavigationStack.Count == 0 ? null : router.NavigationStack[router.NavigationStack.Count - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<object?> CreateRouterViewModelObservable(RoutingState router) =>
        Signal.Return(GetCurrentViewModel(router))
            .Merge(router.CurrentViewModel.Select(static viewModel => (object?)viewModel));

    private MultipleDisposable CreateNavigationDisposables(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var routerChanges = this.GetObservable(RouterProperty);
        var viewContract = this.GetObservable(ViewContractProperty);
        var viewModelSources = routerChanges
            .Select(static router => router is null ? Signal.Return<object?>(null) : CreateRouterViewModelObservable(router));
#if REACTIVE_SHIM
        var viewModels = System.Reactive.Linq.Observable.Switch(viewModelSources);
#else
        var viewModels = viewModelSources.SwitchTo();
#endif
        var navigation = viewModels
            .CombineLatest(viewContract, static (viewModel, contract) => new NavigationTarget(viewModel, contract));

        return ViewHostNavigation.Subscribe(navigation, NavigateToViewModel);
    }
}

internal static class ViewHostNavigation
{
    private static readonly ConcurrentDictionary<Type, Type?> LayoutTypes = new();
    
    internal static IViewFor? ResolveView<THost>(
        THost host, 
        object viewModel, 
        string? contract,
        IViewLocator? viewLocator)
        where THost : class, IEnableLogger
    {
        var locator = viewLocator ?? ViewLocator.GetCurrent();
        var viewInstance = locator.ResolveView(viewModel, contract);
        if (viewInstance is null)
        {
            LogMissingView(host, viewModel, contract);
            return null;
        }

        host.Log().Info(contract is null
            ? $"Ready to show {viewInstance} with autowired {viewModel}."
            : $"Ready to show {viewInstance} with autowired {viewModel} and contract '{contract}'.");

        viewInstance.ViewModel = viewModel;
        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModel;
        }

        if (host is LayoutViewHost layoutViewHost)
        {
            var layout = GetLayoutControl(
                layoutViewHost,
                viewInstance.GetType(),
                locator);

            if (layout != null)
            {
                layout.Content = viewInstance;
                viewInstance = layout;
            }
        }
        
        return viewInstance;
    }
    
    private static ReactiveContentControlBase? GetLayoutControl(
        LayoutViewHost viewHost,
        Type view,
        IViewLocator locator)
    {
        var provider = viewHost.Provider ??
                       AppLocator.Current.GetService<IServiceProvider>() ??
                       throw new NullReferenceException(typeof(IServiceProvider).FullName);
        
        var attribute = view.GetCustomAttribute<LayoutHostAttribute>();

        if (attribute is null)
            return null;
        
        var viewModelType = GetViewModelType(attribute.LayoutControl);

        if (viewModelType is null)
            throw new NotSupportedException(
                $"Unsupported layout type: {attribute.LayoutControl.FullName}");
        
        if (viewHost.CurrentLayout == null || (viewHost.Content is IDataContextProvider context && context.DataContext?.GetType() != viewModelType))
            viewHost.CurrentLayout = ActivatorUtilities.CreateInstance(
                provider,
                viewModelType,
                viewHost.MainScreen);

        var layoutInstance = locator.ResolveView(viewHost.CurrentLayout);

        if (layoutInstance is not ReactiveContentControlBase control)
            throw new InvalidOperationException(
                $"Layout must resolve to {nameof(ReactiveContentControlBase)}.");

        control.DataContext = viewHost.CurrentLayout;

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
    
    internal static MultipleDisposable Subscribe(IObservable<NavigationTarget> navigation, Action<object?, string?> navigate)
    {
        var subscription = LinqExtensions.SubscribeSafe(
            navigation,
            target => navigate(target.ViewModel, target.Contract),
            Throw);

        return new() { subscription };
    }
    
    internal static void Throw(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

    internal static void Release(ref MultipleDisposable? disposables)
    {
        var pending = disposables;
        disposables = null;
        pending?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogMissingView<THost>(THost host, object viewModel, string? contract)
        where THost : class, IEnableLogger =>
        host.Log().Warn(contract is null
            ? $"Couldn't find view for '{viewModel}'. Is it registered? Falling back to default content."
            : $"Couldn't find view with contract '{contract}' for '{viewModel}'. Is it registered? Falling back to default content.");
}

internal readonly record struct NavigationTarget(object? ViewModel, string? Contract);