using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static EnumsLib.Enums;

namespace BankingSystem
{
    class InvestmentPropertiesConverter : IValueConverter
    {
        public static InvestmentPropertiesConverter Instance = new InvestmentPropertiesConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            AbstractClient client = null;
            if (value is Individual)
                client = (Individual)value;
            else if (value is VIPClient)
                client = (VIPClient)value;
            else client = (Juridical)value;

            switch ((string)parameter)
            {
                case "CurrentDate": return client.Investment.CurrentDate.ToShortDateString();
                case "Sum": return $"{client.Investment.InvestmentSum}$";
                case "Date": return client.Investment.InvestmentDate.ToShortDateString();
                case "Type": return client.Investment.Type == InvestmentType.Capitalization ? "С капитализацией" : "Без капитализации";
                case "Percentage": return $"{client.Investment.Percentage}%";
                case "CurrentSum": return $"{client.Investment.CurrentSum}$";
                default:
                    break;
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
