﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    class Juridical : AbstractClient
    {
        public Juridical(string name, string lastname, string patronymic, ClientType type, int age, long cardnumber, long balance, Investment investment)
            : base(name, lastname, patronymic, type, age, cardnumber, balance, investment)
        {

        }
    }
}