using ReactiveUI;

namespace Stoat.Client.ViewModel;

public partial class MainViewModel : ViewModelBase, IScreen
{
    public RoutingState Router { get; } = new();
    
    protected override void SetupRx()
    {
        
    }
}