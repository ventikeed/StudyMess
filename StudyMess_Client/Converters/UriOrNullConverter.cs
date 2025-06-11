using System.Globalization;
using System.Windows.Data;

namespace StudyMess_Client.Converters
{
    public class UriOrNullConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                return Uri.TryCreate(s, UriKind.Absolute, out var uri) ? uri : null;
            }
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}