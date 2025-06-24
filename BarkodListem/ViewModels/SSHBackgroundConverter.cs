using System.Globalization;
namespace BarkodListem.Converters
{
    public class SSHBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool SORUNVAR && SORUNVAR)
                return Color.FromArgb("#FFF9C4"); // Sarımsı renk
            return Color.FromArgb("#f5f5f5"); // Normal gri
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
