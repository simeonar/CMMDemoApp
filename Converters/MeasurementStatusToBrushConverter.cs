using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CMMDemoApp.Models;

namespace CMMDemoApp.Converters
{
    public class MeasurementStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MeasurementStatus status)
            {
                return status switch
                {
                    MeasurementStatus.NotStarted => new SolidColorBrush(Colors.Gray),
                    MeasurementStatus.InProgress => new SolidColorBrush(Colors.Orange),
                    MeasurementStatus.Completed => new SolidColorBrush(Colors.Green),
                    MeasurementStatus.Failed => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
