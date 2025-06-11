using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StudyMess_Client.Converters
{
    public class TextAndIsMineToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var text = values[0] as string;
            var isMine = values[1] as bool? ?? false;
            return !string.IsNullOrWhiteSpace(text) && isMine ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
