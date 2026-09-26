using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives.Disposables;

namespace Stoat.Theme.Controls.ReactiveUI;

public class ReactiveContentControlBase : ContentControl, IViewFor
{
    public static readonly StyledProperty<object?> ViewModelProperty = AvaloniaProperty.Register<ReactiveContentControlBase, object?>(
        nameof(IViewFor.ViewModel));

    public ReactiveContentControlBase()
    {
        _ = this.WhenActivated(static (MultipleDisposable disposables) => { });
    }

    object? IViewFor.ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        base.OnPropertyChanged(change);
        
        if (change.Property == DataContextProperty 
                && ReferenceEquals(change.OldValue, GetValue(ViewModelProperty))
                && IsValidViewModelValue(change.NewValue))
            SetCurrentValue(ViewModelProperty, change.NewValue);
        else if(change.Property == ViewModelProperty
                && ReferenceEquals(change.OldValue, DataContext))
            SetCurrentValue(DataContextProperty, change.NewValue);
    }
    
    protected virtual bool IsValidViewModelValue(object? value) => true;
}

public class ReactiveContentControl<TViewModel> : ReactiveContentControlBase, IViewFor<TViewModel>
    where TViewModel : class
{
    public ReactiveContentControl()
    {
    }
    
    public TViewModel? ViewModel
    {
        get => (TViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }
    
    protected override bool IsValidViewModelValue(object? value) => value is null or TViewModel;
}