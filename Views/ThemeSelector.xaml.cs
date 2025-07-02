using System.Windows;
using System.Windows.Controls;
using CMMDemoApp.Helpers;

namespace CMMDemoApp.Views
{
    public partial class ThemeSelector : UserControl
    {
        public ThemeSelector()
        {
            InitializeComponent();
            Loaded += ThemeSelector_Loaded;
        }

        private void ThemeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            // Set Modern Fluent as default
            ThemeComboBox.SelectedIndex = 0;
            ApplySelectedTheme();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySelectedTheme();
        }

        private void ApplySelectedTheme()
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? themeName = selectedItem.Tag as string ?? selectedItem.Content as string;
                if (!string.IsNullOrEmpty(themeName))
                {
                    ThemeManager.ApplyTheme(themeName);
                }
            }
        }
    }
}
