using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using static StudyMess_Client.Services.UrlStorage;

namespace StudyMess_Client.Converters
{
    public class NullToDefaultAvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
                return new BitmapImage(new Uri(url));
            return new BitmapImage(new Uri($"{BaseUrl}/avatars/logo.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}