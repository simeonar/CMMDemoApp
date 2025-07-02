using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMDemoApp.Helpers;

namespace CMMDemoApp.ViewModels
{
    public partial class ThemeSelectorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _selectedTheme;

        public ThemeSelectorViewModel()
        {
            Themes = new List<string>
            {
                "Modern Fluent",
                "Minimalist Scientific",
                "Industrial Professional",
                "Dark Technical"
            };
            SelectedTheme = Themes.FirstOrDefault();
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
            Debug.WriteLine($"[ThemeSelectorViewModel] OnSelectedThemeChanged called with value: {value}");
            if (!string.IsNullOrEmpty(value))
            {
                ThemeManager.ApplyTheme(value);
            }
        }
    }
}
