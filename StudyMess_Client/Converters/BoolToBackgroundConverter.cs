using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StudyMess_Client.Converters
{
    public class BoolToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b)
                ? new SolidColorBrush(Color.FromRgb(209, 245, 211))
                : new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
