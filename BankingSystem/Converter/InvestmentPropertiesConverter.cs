using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            DataRowView investment = (DataRowView)value;
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
