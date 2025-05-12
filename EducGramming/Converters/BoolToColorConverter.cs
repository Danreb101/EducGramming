using System.Globalization;

namespace EducGramming.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colors)
            {
                var colorParts = colors.Split(':');
                if (colorParts.Length == 2)
                {
                    var trueColor = colorParts[0];
                    var falseColor = colorParts[1];
                    
                    if (boolValue)
                    {
                        return trueColor.StartsWith("#") ? 
                            Color.FromArgb(trueColor) : 
                            GetNamedColor(trueColor);
                    }
                    else
                    {
                        return falseColor.StartsWith("#") ? 
                            Color.FromArgb(falseColor) : 
                            GetNamedColor(falseColor);
                    }
                }
            }
            
            return Colors.White;
        }

        private Color GetNamedColor(string colorName)
        {
            return colorName.ToLowerInvariant() switch
            {
                "white" => Colors.White,
                "black" => Colors.Black,
                "red" => Colors.Red,
                "green" => Colors.Green,
                "blue" => Colors.Blue,
                "yellow" => Colors.Yellow,
                "gray" or "grey" => Colors.Gray,
                "transparent" => Colors.Transparent,
                _ => Colors.White // default to white
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 