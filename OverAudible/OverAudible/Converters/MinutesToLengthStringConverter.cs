using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverAudible.Converters
{
    public class MinutesToLengthStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d )
            {
                var t = TimeSpan.FromMinutes(d);
                return $"Length: {t.Hours} hrs and {t.Minutes} minutes";
            }

            if (value is int i)
            {
                double d1 = i / 60;
                var t = TimeSpan.FromHours(d1);
                return $"Length: {(t.Days * 24) + t.Hours} hrs and {t.Minutes} minutes";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
