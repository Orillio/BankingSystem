using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BankingSystem
{
    [ValueConversion(typeof(int), typeof(string))]
    public class CardConverter : IValueConverter
    {
        public static CardConverter Instance = new CardConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (long.TryParse(value.ToString(), out var num)) 
                return $"{value:#### #### #### ####}";
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
