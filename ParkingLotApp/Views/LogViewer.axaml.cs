using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ParkingLotApp.Views
{
    public partial class LogViewer : StackPanel
    {
        public LogViewer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 