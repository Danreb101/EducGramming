using System.Globalization;

namespace EducGramming.Converters
{
    public class AnswerToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isWrong)
            {
                // Return red for wrong answers, green for correct
                return isWrong ? Colors.Red : Colors.Green;
            }
            else if (value is string answer && parameter is string wrongAnswer)
            {
                if (string.IsNullOrEmpty(wrongAnswer))
                    return Colors.White;
                    
                return answer == wrongAnswer ? Color.FromArgb("#FFE0E0") : Colors.White;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 