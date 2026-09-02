using ReactiveUI;

namespace Stoat.Client.ViewModel;

public abstract class ViewModelBase : ReactiveObject
{
    public ViewModelBase()
    {
        SetupRx();
    }

    protected virtual void SetupRx()
    {
        
    }
}