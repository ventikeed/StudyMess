using System.Globalization;
using System.Windows.Data;

namespace StudyMess_Client.Converters
{
    public class BoolToTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine)
                return isMine ? System.Windows.TextAlignment.Right : System.Windows.TextAlignment.Left;
            return System.Windows.TextAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
