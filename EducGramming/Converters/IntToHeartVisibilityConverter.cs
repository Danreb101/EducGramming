using System.Globalization;

namespace EducGramming.Converters
{
    public class IntToHeartVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int lives && parameter is string param && int.TryParse(param, out int heartPosition))
            {
                // Simple visibility check: show heart if lives is greater than or equal to position
                return lives >= heartPosition;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 