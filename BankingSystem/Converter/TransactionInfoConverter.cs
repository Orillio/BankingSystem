using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BankingSystem
{
    [ValueConversion(typeof(TransactionInfo), typeof(string))]
    class TransactionInfoConverter : IValueConverter
    {
        public static TransactionInfoConverter Instance = new TransactionInfoConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var trans = value as TransactionInfo;
            if (parameter.Equals("Client"))
            {
                switch (trans.ClientTypeTarget)
                {
                    case ClientType.VIP: return "VIP Клиент";
                    case ClientType.Individual: return "Физическое лицо";
                    case ClientType.Juridical: return "Юридическое лицо";
                    default:
                        break;
                }
            }
            else if (parameter.Equals("Type"))
            {
                switch (trans.Type)
                {
                    case TransactionType.Payment: return "Платеж";
                    case TransactionType.Receive: return "Получение";
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
