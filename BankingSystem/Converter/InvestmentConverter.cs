using System;
using System.Globalization;
using System.Windows.Data;
using InvestmentLib;
using static EnumsLib.Enums;

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
