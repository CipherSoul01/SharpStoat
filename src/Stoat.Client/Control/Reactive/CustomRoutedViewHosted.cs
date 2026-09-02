using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;

namespace Stoat.Client.Control.Reactive;

public class CustomRoutedViewHosted : TransitioningContentControl, IActivatableView, IEnableLogger
{
    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<CustomRoutedViewHosted, RoutingState?>(nameof(Router));

    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<CustomRoutedViewHosted, string?>(nameof(ViewContract));

    public static readonly StyledProperty<object?> DefaultContentProperty =
        ViewModelViewHost.DefaultContentProperty.AddOwner<CustomRoutedViewHosted>();


    public CustomRoutedViewHosted()
    {
        this.WhenActivated(disposables =>
        {
            var routerRemoved = this
                .WhenAnyValue(x => x.Router)
                .Where(router => router == null)
                .Cast<object?>();

            var viewContract = this
                .WhenAnyValue(x => x.ViewContract);


            this.WhenAnyValue(x => x.Router)
                .Where(router => router != null)
                .SelectMany(router => router!.CurrentViewModel)
                .Merge(routerRemoved)
                .CombineLatest(viewContract)
                .SelectMany(tuple =>
                    Observable.FromAsync(() =>
                        NavigateToViewModel(
                            tuple.First,
                            tuple.Second)))
                .Subscribe()
                .DisposeWith(disposables);
        });
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


    public IViewLocator? ViewLocator { get; set; }


    protected override Type StyleKeyOverride =>
        typeof(TransitioningContentControl);


    private Task NavigateToViewModel(
        object? viewModel,
        string? contract)
    {
        if (Router == null)
        {
            Content = DefaultContent;
            return Task.CompletedTask;
        }

        if (viewModel == null)
        {
            // Aqui sim: estamos removendo a página atual.
            IsTransitionReversed = true;
            Content = DefaultContent;

            return Task.CompletedTask;
        }

        var viewLocator =
            ViewLocator ?? ReactiveUI.ViewLocator.Current;

        var viewInstance =
            viewLocator.ResolveView(viewModel, contract);

        if (viewInstance == null)
        {
            // Falha de resolução, não é "back".
            this.Log()
                .Warn($"Couldn't find view for '{viewModel}'.");

            return Task.CompletedTask;
        }

        viewInstance.ViewModel = viewModel;

        if (viewInstance is IDataContextProvider provider)
            provider.DataContext = viewModel;

        IsTransitionReversed = false;
        Content = viewInstance;

        return Task.CompletedTask;
    }
}