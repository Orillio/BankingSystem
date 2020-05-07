﻿using BankingSystem.Departments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    class Department<T> : BaseDepartment
        where T : AbstractClient
    {
        public string Name { get; set; }
        public ObservableCollection<T> Clients { get; set; }

        public Department(string name)
        {
            this.Name = name;
            this.Clients = new ObservableCollection<T>();
        }
    }
}