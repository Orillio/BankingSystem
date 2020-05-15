using System.Collections.ObjectModel;
using PropertiesChangedLib;

namespace BankingSystem
{
    public class BaseDepartment : PropertiesChanged { }
    public class Department<T> : BaseDepartment
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
