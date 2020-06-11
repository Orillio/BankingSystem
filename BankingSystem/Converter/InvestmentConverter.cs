using System;
using System.Globalization;
using System.Windows.Data;
using InvestmentLib;
using static EnumsLib.Enums;

namespace BankingSystem
{
    class InvestmentConverter : IValueConverter
    {
        public static InvestmentConverter Instance = new InvestmentConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var clientId = (int)value;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
