using ReactiveUI;


namespace Stoat.Client.ViewModel;

public partial class MainViewModel
{
    private object? _dialog;
    public object? Dialog
    {
        get => _dialog; 
        set => this.RaiseAndSetIfChanged(ref _dialog, value);
    }

}