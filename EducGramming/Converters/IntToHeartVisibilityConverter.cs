using System.Globalization;

namespace EducGramming.Converters
{
    public class IntToHeartVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int lives && parameter is string param && int.TryParse(param, out int heartPosition))
            {
                // Show heart if lives is greater than or equal to position
                // Heart3 shows when lives >= 3
                // Heart2 shows when lives >= 2
                // Heart1 shows when lives >= 1
                return lives >= heartPosition;
            }
            return true; // Default to visible if something goes wrong
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 