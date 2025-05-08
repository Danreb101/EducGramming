using System.Globalization;

namespace EducGramming.Converters
{
    public class TabToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentTab && parameter is string tabName)
            {
                return currentTab == tabName ? Color.FromArgb("#FF3B30") : Color.FromArgb("#1B3B6F");
            }
            return Color.FromArgb("#1B3B6F");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 