using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LCDonald.Views
{
    public partial class SettingsPopup : UserControl
    {
        public SettingsPopup()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
