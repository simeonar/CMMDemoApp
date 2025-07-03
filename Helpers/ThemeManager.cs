using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CMMDemoApp.Helpers
{
    public static class ThemeManager
    {
        private static ResourceDictionary? _currentThemeDictionary;
        private static string? _currentThemeName;

        public static event EventHandler<string>? ThemeChanged;

        // Dictionary containing themes with their display names and resource paths
        public static readonly Dictionary<string, string> AvailableThemes = new Dictionary<string, string>
        {
            { "Modern Fluent", "/Themes/ModernFluentTheme.xaml" },
            { "Minimalist Scientific", "/Themes/MinimalistScientificTheme.xaml" },
            { "Industrial Professional", "/Themes/IndustrialProfessionalTheme.xaml" },
            { "Dark Technical", "/Themes/DarkTechnicalTheme.xaml" }
        };

        public static string? CurrentTheme => _currentThemeName;

        // Apply the selected theme to the application
        public static bool ApplyTheme(string themeName)
        {
            try
            {
                if (string.IsNullOrEmpty(themeName))
                {
                    System.Diagnostics.Debug.WriteLine("[ThemeManager] Theme name is null or empty");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Applying theme: {themeName}");

                if (!AvailableThemes.TryGetValue(themeName, out var themePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] Theme not found: {themeName}");
                    return false;
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
                _currentThemeName = themeName;

                ThemeChanged?.Invoke(null, themeName);
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Theme applied successfully: {themeName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Error applying theme: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
