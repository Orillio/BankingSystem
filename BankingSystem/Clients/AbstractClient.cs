using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    public abstract class AbstractClient : PropertiesChanged
    {
        public ClientType ClientType { get; set; }
        public string FirstName{ get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public int Age { get; set; }
        public long BankBalance { get; set; }
        public long CardNumber { get; set; }
        public Investment Investment { get; set; }

        public AbstractClient(string name, string lastname, string patronymic, ClientType clientType, int age)
        {
            this.ClientType = clientType;
            this.FirstName = name;
            this.LastName = lastname;
            this.Patronymic = patronymic;
            this.Age = age;
            this.Investment = null;
            this.BankBalance = 0;
            this.CardNumber = long.Parse(Bank.CardRandom());
        }

    }
}
