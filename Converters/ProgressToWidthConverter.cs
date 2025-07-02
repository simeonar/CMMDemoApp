using System;
using System.Globalization;
using System.Windows.Data;

namespace CMMDemoApp.Converters
{
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 || 
                values[0] is not double value ||
                values[1] is not double minimum ||
                values[2] is not double maximum ||
                values[3] is not double trackWidth)
            {
                return 0.0;
            }

            if (maximum <= minimum || value < minimum)
                return 0.0;

            if (value > maximum)
                value = maximum;

            return (value - minimum) / (maximum - minimum) * trackWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
