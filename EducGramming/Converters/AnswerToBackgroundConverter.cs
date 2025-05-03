using System.Globalization;

namespace EducGramming.Converters
{
    public class AnswerToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string answer && parameter is string wrongAnswer)
            {
                if (string.IsNullOrEmpty(wrongAnswer))
                    return Colors.White;
                    
                return answer == wrongAnswer ? Colors.LightPink : Colors.White;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 