using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BankingSystem.DataBase;
using Newtonsoft.Json;

namespace BankingSystem
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Сортировка репозитория клиентов по ключу и выбранному типу сортировки
        /// </summary>
        /// <typeparam name="TClient">Входящий тип клиента</typeparam>
        /// <typeparam name="TKey">Тип ключ для сортирвки</typeparam>
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
        public static void Update(this IDataBase db) =>
            db.Adapter.Update(db.Table);
    }
}
