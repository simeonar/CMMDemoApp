using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CMMDemoApp.Models;

namespace CMMDemoApp.Converters
{
    public class MeasurementStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MeasurementStatus status)
            {
                return status switch
                {
                    MeasurementStatus.NotStarted => Visibility.Visible,
                    MeasurementStatus.Failed => Visibility.Visible,
                    _ => Visibility.Collapsed
                };
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
