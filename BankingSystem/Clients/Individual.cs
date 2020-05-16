using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EnumsLib.Enums;

namespace BankingSystem
{
    public class Individual : AbstractClient
    {
        public Individual(string name, string lastname, string patronymic, ClientType type, int age)
            : base(name, lastname, patronymic, type, age)
        {

        }
        
    }
}
