using System;
using System.Collections.Generic;
using System.Windows;

namespace CMMDemoApp.Helpers
{
    public class ThemeManager
    {
        // Dictionary containing themes with their display names and resource paths
        public static readonly Dictionary<string, string> AvailableThemes = new Dictionary<string, string>
        {
            { "Minimalist Scientific", "/Themes/MinimalistScientificTheme.xaml" },
            { "Industrial Professional", "/Themes/IndustrialProfessionalTheme.xaml" },
            { "Dark Technical", "/Themes/DarkTechnicalTheme.xaml" },
            { "Modern Fluent", "/Themes/ModernFluentTheme.xaml" }
        };

        private static ResourceDictionary? _currentTheme;

        // Apply the selected theme to the application
        public static void ApplyTheme(string themeName)
        {
            try
            {
                if (!AvailableThemes.ContainsKey(themeName))
                {
                    return;
                }

                var themePath = AvailableThemes[themeName];
                
                var resourceDict = new ResourceDictionary
                {
                    Source = new Uri(themePath, UriKind.Relative)
                };

                // Remove current theme if exists
                if (_currentTheme != null && Application.Current.Resources.MergedDictionaries.Contains(_currentTheme))
                {
                    Application.Current.Resources.MergedDictionaries.Remove(_currentTheme);
                }

                // Add new theme
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                _currentTheme = resourceDict;
                
                // Force UI refresh for all windows
                foreach (Window window in Application.Current.Windows)
                {
                    // Trigger update of DynamicResource references
                    var temp = window.Resources;
                    
                    // Force re-evaluation of styles and templates
                    window.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                // Silent error handling for production
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }
    }
}
