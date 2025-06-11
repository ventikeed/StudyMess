using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StudyMess_Client.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter?.ToString() == "Inverse";
            bool isNullOrEmpty = string.IsNullOrWhiteSpace(value?.ToString());
            if (inverse)
                isNullOrEmpty = !isNullOrEmpty;
            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}