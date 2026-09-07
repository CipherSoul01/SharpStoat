using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Stoat.Client.Theme;

namespace Stoat.Client.View.App.Hub
{
    public partial class HubView : UserControl
    {
        public HubView()
        {
            if (Design.IsDesignMode)
                ThemeService.Instance.SetTheme(StoatThemeVariants.Dark);
            InitializeComponent();
        }
    }
}