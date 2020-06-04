using PropertiesChangedLib;
using static EnumsLib.Enums;

namespace TransactionLib
{
    public enum TransactionType { Payment = 1, Receive = 2}
    public class TransactionInfo : PropertiesChanged
    { 
        public string NameTarget { get; set; }
        public string LastnameTarget { get; set; }
        public string PatronymicTarget { get; set; }
        public long CardTarget { get; set; }
        public ClientType ClientTypeTarget { get; set; }
        public long TransactionSum { get; set; }
        public TransactionType Type { get; set; }
        public TransactionInfo(string name, string last, string patr, long card, long sum, ClientType cltype)
        {
            this.ClientTypeTarget = cltype;
            this.NameTarget = name;
            this.LastnameTarget = last;
            this.PatronymicTarget = patr;
            this.CardTarget = card;
            this.TransactionSum = sum;
        }
    }
}
