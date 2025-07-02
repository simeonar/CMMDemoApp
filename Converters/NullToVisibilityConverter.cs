using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CMMDemoApp.Converters
{
    /// <summary>
    /// Converts a null value to Visibility.Collapsed and a non-null value to Visibility.Visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
