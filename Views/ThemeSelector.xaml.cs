using System.Windows;
using System.Windows.Controls;
using CMMDemoApp.Helpers;
using CMMDemoApp.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace CMMDemoApp.Views
{
    public partial class ThemeSelector : UserControl
    {
        public ThemeSelector()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<ThemeSelectorViewModel>();
        }
    }
}
