using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BankingSystem
{
    public class CheckingAccountConverter : IValueConverter
    {
        public static CheckingAccountConverter Instance { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedDep = (int)value;
            if (selectedDep == 1)
            {
                return "Рассчетный счет";
            }
            else return "Номер карты";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
