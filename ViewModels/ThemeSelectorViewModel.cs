using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMDemoApp.Helpers;
using System.Windows;

namespace CMMDemoApp.ViewModels
{
    public partial class ThemeSelectorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _selectedTheme;

        public ThemeSelectorViewModel()
        {
            Debug.WriteLine("[ThemeSelectorViewModel] Initializing...");
            try
            {
                Themes = ThemeManager.AvailableThemes.Keys.ToList();
                SelectedTheme = Themes.FirstOrDefault();
                Debug.WriteLine($"[ThemeSelectorViewModel] Loaded {Themes.Count} themes, selected: {SelectedTheme}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ThemeSelectorViewModel] Error initializing: {ex.Message}");
                Themes = new List<string>();
            }
        }

        public List<string> Themes { get; }

        [RelayCommand]
        private void ApplyTheme()
        {
            if (!string.IsNullOrEmpty(SelectedTheme))
            {
                ThemeManager.ApplyTheme(SelectedTheme);
            }
        }

        partial void OnSelectedThemeChanged(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.WriteLine("[ThemeSelectorViewModel] OnSelectedThemeChanged: value is null or empty");
                return;
            }

            try
            {
                Debug.WriteLine($"[ThemeSelectorViewModel] Applying theme: {value}");
                ThemeManager.ApplyTheme(value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ThemeSelectorViewModel] Error applying theme: {ex.Message}");
                MessageBox.Show($"Error applying theme: {ex.Message}", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
