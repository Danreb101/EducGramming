using System.Globalization;

namespace EducGramming.Converters
{
    public class PasswordVisibilityIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? "eye_off.svg" : "eye.svg";
            }
            return "eye.svg";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
} 