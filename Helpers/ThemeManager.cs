using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CMMDemoApp.Helpers
{
    public static class ThemeManager
    {
        private static ResourceDictionary? _currentThemeDictionary;

        // Dictionary containing themes with their display names and resource paths
        public static readonly Dictionary<string, string> AvailableThemes = new Dictionary<string, string>
        {
            { "Modern Fluent", "/Themes/ModernFluentTheme.xaml" },
            { "Minimalist Scientific", "/Themes/MinimalistScientificTheme.xaml" },
            { "Industrial Professional", "/Themes/IndustrialProfessionalTheme.xaml" },
            { "Dark Technical", "/Themes/DarkTechnicalTheme.xaml" }
        };

        // Apply the selected theme to the application
        public static void ApplyTheme(string themeName)
        {
            if (!AvailableThemes.TryGetValue(themeName, out var themePath))
            {
                return;
            }

            var newThemeDict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };

            // Remove the current theme dictionary if it exists
            if (_currentThemeDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_currentThemeDictionary);
            }

            // Add the new theme dictionary and update the reference
            Application.Current.Resources.MergedDictionaries.Add(newThemeDict);
            _currentThemeDictionary = newThemeDict;
        }
    }
}
