using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace BankingSystem
{
    public class BaseDepartment { }
    [AddINotifyPropertyChangedInterface]
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
