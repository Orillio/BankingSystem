using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace BankingSystem
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Сортировка репозитория клиентов по ключу и выбранному типу сортировки
        /// </summary>
        /// <typeparam name="TClient">Входящий тип клиента</typeparam>
        /// <typeparam name="TKey">Ключ для сортирвки</typeparam>
        /// <param name="dep">Департмент для сортировки в нем клиентов</param>
        /// <param name="func"></param>
        /// <param name="descending">Тип сортировки. true - по возрастанию. false - по убыванию</param>
        /// <returns></returns>
        public static void Order<TClient, TKey>(this Department<TClient> dep, Func<TClient, TKey> func, bool descending)
            where TClient : AbstractClient =>

            dep.Clients = descending 
                ? new ObservableCollection<TClient>(dep.Clients.OrderBy(func))
                : new ObservableCollection<TClient>(dep.Clients.OrderByDescending(func));

        public static void RemoveFrom<T>(this AbstractClient client, Department<T> department)
            where T : AbstractClient => department.Clients.Remove(client as T);

        public static void AddTo<T>(this AbstractClient client, Department<T> department)
            where T : AbstractClient => department.Clients.Add(client as T);

        //public static void JsonSerializer(this Bank bank)
        //{
        //    if (!Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs"))
        //        Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs");

        //    bank.Load = new LoadingScreen();
        //    bank.Load.Show();

        //    var thread = new Thread(() =>
        //    {
        //        var task1 = Task.Run(() =>
        //        {
        //            var json = JsonConvert.SerializeObject(bank.DepItems[0] as Department<Juridical>);
        //            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs\Juridical.json", json);
        //        });
        //        var task2 = Task.Run(() =>
        //        {
        //            var json1 = JsonConvert.SerializeObject(bank.DepItems[1] as Department<Individual>);
        //            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs\Individual.json", json1);
        //        });
        //        var task3 = Task.Run(() =>
        //        {
        //            var json2 = JsonConvert.SerializeObject(bank.DepItems[2] as Department<VIPClient>);
        //            File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\BankLogs\VIP.json", json2);
        //        });
        //        Task.WaitAll(task1, task2, task3);
        //        task1.Dispose(); task2.Dispose(); task3.Dispose();
        //        Application.Current.Dispatcher.Invoke(() => { bank.Load.Close(); });
        //    });
        //    thread.Start();
        //}
    }
}
