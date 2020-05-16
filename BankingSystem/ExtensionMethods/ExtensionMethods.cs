using System;
using System.IO;
using Newtonsoft.Json;

namespace BankingSystem
{
    public static class ExtensionMethods
    {
        public static void RemoveFrom<T>(this AbstractClient client, Department<T> department)
            where T : AbstractClient
        {
            department.Clients.Remove(client as T);
        }
        public static void AddTo<T>(this AbstractClient client, Department<T> department)
            where T : AbstractClient
        {
            department.Clients.Add(client as T);
        }
        public static void JsonSerializer(this Bank bank)
        {
            if (!Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs"))
                Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs");

            var json = JsonConvert.SerializeObject(bank.DepItems[0] as Department<Juridical>);
            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs\Juridical.json", json);
            var json1 = JsonConvert.SerializeObject(bank.DepItems[1] as Department<Individual>);
            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs\Individual.json", json1);
            var json2 = JsonConvert.SerializeObject(bank.DepItems[2] as Department<VIPClient>);
            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs\VIP.json", json2);
        }
    }
}
