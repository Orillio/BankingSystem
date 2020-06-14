using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TransactionLib;
using static EnumsLib.Enums;

namespace BankingSystem
{
    [ValueConversion(typeof(DataRowView), typeof(string))]
    class TransactionInfoConverter : IValueConverter
    {
        public static TransactionInfoConverter Instance = new TransactionInfoConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var trans = value as DataRowView;
            if (parameter.Equals("Client"))
            {
               switch (trans.Row["ClientTypeTarget"])
               {
                   case "VIP": return "VIP Клиент";
                   case "Individual": return "Физическое лицо";
                   case "Juridical": return "Юридическое лицо";
                   default:
                       break;
               }
            }
            else if (parameter.Equals("Type"))
            {
                switch ((int)trans.Row["Type"])
                {
                    case (int)TransactionType.Payment: return "Исходящий";
                    case (int)TransactionType.Receive: return "Входящий";
                    default:
                        break;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
