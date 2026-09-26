using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Extensions;
using ReactiveUI.Primitives.Signals;
using Stoat.Theme.Interfaces.Routing;

namespace Stoat.Theme.Controls.ReactiveUI;

public class LayoutState : ReactiveObject
{
    private readonly ISequencer _scheduler;

    private ILayoutViewModel? _current;

    public LayoutState(ISequencer? scheduler = null)
    {
        _scheduler = scheduler ?? RxSchedulers.MainThreadScheduler;
        SetupRx();
    }

    public ILayoutViewModel? Current
    {
        get => _current;
        private set => this.RaiseAndSetIfChanged(ref _current, value);
    }

    public ReactiveCommand<ILayoutViewModel, ILayoutViewModel> SetLayoutCommand { get; private set; }

    [MemberNotNull(nameof(SetLayoutCommand))]
    private void SetupRx()
    {
        SetLayoutCommand =
            ReactiveCommand.CreateFromObservable<ILayoutViewModel, ILayoutViewModel>(
                vm =>
                {
                    Current = vm;

                    return Signal.Emit(vm)
                        .ObserveOnSafe(_scheduler);
                });
    }
}