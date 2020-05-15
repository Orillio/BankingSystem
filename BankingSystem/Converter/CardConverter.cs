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
            long cardnum = (long)value;
            return $"{cardnum:#### #### #### ####}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
