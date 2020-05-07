using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BankingSystem
{
    [ValueConversion(typeof(Investment), typeof(string))]
    class InvestmentConverter : IValueConverter
    {
        public static InvestmentConverter Instance = new InvestmentConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invest = (Investment)value;
            if (invest == null) return "Отсутствует";
            return invest.Type == InvestmentType.Capitalization ? "С капитализацией" : "Без капитализации";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
