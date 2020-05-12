﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    public enum TransactionType { Payment, Receive }
    public class TransactionInfo : PropertiesChanged
    { 
        public string NameTarget { get; set; }
        public string LastnameTarget { get; set; }
        public string PatronymicTarget { get; set; }
        public long CardTarget { get; set; }
        public ClientType ClientTypeTarget { get; set; }
        public long TransactionSumWithPercent { get; set; }
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