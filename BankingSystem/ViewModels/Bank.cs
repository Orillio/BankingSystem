using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace BankingSystem
{
    enum OrderType { Name, NameDesc, Lastname, LastnameDesc, Age, AgeDesc, Balance, BalanceDesc }
    class Bank : PropertiesChanged // Модель представления банка для основногo окна
    {
        #region Рандом
        public static Random Random { get; set; } = new Random();

        #endregion

        #region Коллекция отделов
        public ObservableCollection<BaseDepartment> DepItems { get; set; } // Коллекция с 3 отделами 
        #endregion

        #region Окна
        private ClientInfoWindow Info { get; set; } // окно с информацией 
        private InvestCreateWindow CreateInvest { get; set; }
        private TransferWindow Transfer { get; set; }
        private DepositWindow Deposit { get; set; }
        private AddClientWindow AddClient { get; set; }
        private EditClientWindow EditClient { get; set; }
        #endregion

        #region Переданные данные через View
        public AbstractClient SelectedClient { get; set; } // Информация о выбранном клиенте, которая передается через View
        public BaseDepartment SelectedDepartment { get; set; } // Информация о выбранном отделе, которая передается через View
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public ComboBoxItem SelectedInvType { get; set; }
        public ComboBoxItem SelectedClientType { get; set; }
        public ComboBoxItem EditSelectedClientType { get; set; }
        #endregion

        #region Команды
        public ICommand InvestmentButton { get; set; } 
        public ICommand InfoClick { get; set; }
        public ICommand WithdrawButton { get; set; }
        public ICommand TransferButton { get; set; }
        public ICommand TransferButtonWindow { get; set; }
        public ICommand DepositButton { get; set; }
        public ICommand DepositButtonWindow { get; set; }
        public ICommand AddClientButton { get; set; }
        public ICommand AddClientButtonWindow { get; set; }
        public ICommand EditClientButton { get; set; }
        public ICommand EditClientButtonWindow { get; set; }
        public ICommand DeleteClient { get; set; }
        public ICommand NameClick { get; set; }
        public ICommand LastClick { get; set; }
        public ICommand PatrClick { get; set; }
        public ICommand BalanceClick { get; set; }
        public ICommand TransferInfo { get; set; }

        #endregion

        #region Дополнительные переменные для сортировки
        bool namedesc = false;
        bool lastdesc = false;
        bool agedesc = false;
        bool balancedesc = false;
        #endregion

        #region Конструктор
        public Bank()
        {
            #region Инициализация отделов
            DepItems = new ObservableCollection<BaseDepartment>(); // добавление отделов в коллекцию
            DepItems.Add(new Department<Juridical>("Юридические лица"));
            DepItems.Add(new Department<Individual>("Физические лица"));
            DepItems.Add(new Department<VIPClient>("VIP Клиенты"));

            #endregion

            #region Десериализация

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (File.Exists($@"{path}\BankLogs\Juridical.json") && File.Exists($@"{path}\BankLogs\Individual.json") && File.Exists($@"{path}\BankLogs\VIP.json"))
            {
                DepItems[0] = JuridJsonDeserialize();
                DepItems[1] = IndivJsonDeserialize();
                DepItems[2] = VIPJsonDeserialize();
            }
            else
            {
                FillClients(10);
                JsonSerialize(this);
            }    
            #endregion

            #region Команды

            InfoClick = new Command(ClickInfo); // открытие информационной панели о вкладе
            WithdrawButton = new Command(() =>
            {
                var res = MessageBox.Show($"Вы уверены что хотите вывести вклад? Вы выведите {SelectedClient.Investment.CurrentSum}$", "",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                SelectedClient.BankBalance += SelectedClient.Investment.CurrentSum;
                SelectedClient.Investment = null;
                Info.Close();
                JsonSerialize(this);
                MessageBox.Show("Вы вывели вклад на свой счет.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }); // вывод средств из вклада на счет
            InvestmentButton = new Command(() =>
            {
                #region Обратные условия
                if (string.IsNullOrEmpty(CreateInvest.InvestField.Text)) { MessageBox.Show("Ничего не введено"); return; }
                if (!long.TryParse(CreateInvest.InvestField.Text, out long result)) { MessageBox.Show("Введена строка или слишком большое число"); return; }
                if (SelectedClient.BankBalance < result) { MessageBox.Show($"Недостаточно средств для вклада. Ваши средства: {SelectedClient.BankBalance}$"); return; }
                if (result < 500) { MessageBox.Show($"Минимальное количество средств для вклада - 500$. Ваши средства: {SelectedClient.BankBalance}$"); return; }
                if (SelectedInvType == null) { MessageBox.Show("Вы не выбрали тип вклада"); return; }

                #endregion

                InvestmentType type = (string)SelectedInvType.Content == "С капитализацией" ? InvestmentType.Capitalization : InvestmentType.NotCapitalization;
                ClientType clientType = SelectedClient.GetType() == typeof(Individual) ? ClientType.Individual : SelectedClient.GetType() == typeof(VIPClient) ? ClientType.VIP : ClientType.Juridical;
                SelectedClient.Investment = new Investment(type, clientType, result, CurrentDate);
                SelectedClient.BankBalance = SelectedClient.BankBalance - result;
                CreateInvest.Close();
                JsonSerialize(this);
                MessageBox.Show("Вклад оформлен!");
            }); // окно открытия вклада
            TransferButton = new Command(() =>
            {
                if (this.Transfer != null) return;
                if (SelectedClient == null) return;
                if (SelectedClient.BankBalance <= 10) { MessageBox.Show("Недостаточно денег для перевода. Минимальное количество - 10$"); return; }
                Transfer = new TransferWindow();
                Transfer.Closed += (sender, e) => { this.Transfer = null; };
                Transfer.DataContext = this;
                Transfer.ShowDialog();
            }); // открытие окна перевода средств
            TransferButtonWindow = new Command(() =>
            {
                var enumcard = Transfer.CardNumber.Text.Where(x => x != ' '); // удаляю лишние пробелы (если такие есть)
                string card = null;
                foreach (var item in enumcard) card += item; // Извлекаю элементы из последовательности

                #region Обратные условия

                if (string.IsNullOrEmpty(card)) { MessageBox.Show("Поле пустое"); return; }
                if (!long.TryParse(card, out long result)) { MessageBox.Show("В поле для ввода карты введена строка или слишком большое число"); return; }
                if (!CheckCardNumber(result, out AbstractClient client)) { MessageBox.Show("Такой карты не существует"); return; }
                if (!long.TryParse(Transfer.TransferSum.Text, out long sum)) { MessageBox.Show("В поле для ввода суммы введена строка или слишком большое число"); return; }
                if (SelectedClient.BankBalance < sum) { MessageBox.Show($"Недостаточно средств для перевода. Ваши средства: {SelectedClient.BankBalance}$"); return; }

                #endregion

                MessageBoxResult res;
                long newsum = 0;
                switch (SelectedClient.ClientType)
                {
                    case ClientType.VIP:
                        res = MessageBox.Show($"Комиссия перевода - 0%. Вы переведете клиенту {sum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient.BankBalance -= sum;
                        client.BankBalance += sum;
                        break;
                    case ClientType.Individual:
                        newsum = sum + (sum / 100 * 3);
                        res = MessageBox.Show($"Комиссия перевода - 3%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient.BankBalance -= newsum;
                        client.BankBalance += sum;
                        break;
                    case ClientType.Juridical:
                        newsum = sum + (sum / 100 * 2);
                        res = MessageBox.Show($"Комиссия перевода - 2%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient.BankBalance -= newsum;
                        client.BankBalance += sum;
                        break;
                    default:
                        break;
                }
                Transfer.Close();
                JsonSerialize(this);
                MessageBox.Show("Перевод завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }); // перевод средств на другой счет
            DepositButton = new Command(() =>
            {
                if (SelectedClient == null) return;
                Deposit = new DepositWindow();
                Deposit.DataContext = this;
                Deposit.Closed += (sender, e) => { this.Deposit = null; };
                Deposit.ShowDialog();
            }); // открытие окна пополнения счета
            DepositButtonWindow = new Command(() =>
            {
                if (string.IsNullOrEmpty(Deposit.DepositField.Text)) { MessageBox.Show("Поле пустое"); return; }
                if (!long.TryParse(Deposit.DepositField.Text, out long result)) { MessageBox.Show("В поле ввода строка или число слишком большое"); return; }
                if (result > 10000) { MessageBox.Show("Нельзя пополнить счет на сумму более 10000$ за раз"); return; }
                SelectedClient.BankBalance += result;
                Deposit.Close();
                JsonSerialize(this);
                MessageBox.Show("Баланс пополнен!");
            }); // пополнение счета 
            AddClientButton = new Command(() =>
            {
                AddClient = new AddClientWindow();
                AddClient.DataContext = this;
                AddClient.ShowDialog();

            }); // открытие окна добавления клиента
            AddClientButtonWindow = new Command(() =>
            {
                if (string.IsNullOrEmpty(AddClient.Name.Text) || string.IsNullOrEmpty(AddClient.Lastname.Text) || string.IsNullOrEmpty(AddClient.Patromymic.Text)
                    || string.IsNullOrEmpty(AddClient.Age.Text) || AddClient.Name.Text.Trim(' ') == "" || AddClient.Lastname.Text.Trim(' ') == "" ||
                        AddClient.Patromymic.Text.Trim(' ') == "" || AddClient.Age.Text.Trim(' ') == "") { MessageBox.Show("Одно или несколько полей не введены"); return; }

                if (!int.TryParse(AddClient.Age.Text, out int res)) { MessageBox.Show("Введен неправильный возраст"); return; }
                if (res > 200) { MessageBox.Show("Введен невозможный для человека возраст"); return; }
                if (SelectedClientType == null) { MessageBox.Show("Вы не выбрали тип клиента"); return; }
                AbstractClient newClient = null;
                if (SelectedClientType.Tag.Equals("VIP"))
                {
                    newClient = new VIPClient(AddClient.Name.Text, AddClient.Lastname.Text, AddClient.Patromymic.Text, ClientType.VIP, int.Parse(AddClient.Age.Text));
                    (DepItems[2] as Department<VIPClient>).Clients.Add(newClient as VIPClient);
                }
                else if (SelectedClientType.Tag.Equals("Indiv"))
                {
                    newClient = new Individual(AddClient.Name.Text, AddClient.Lastname.Text, AddClient.Name.Text, ClientType.VIP, int.Parse(AddClient.Age.Text));
                    (DepItems[1] as Department<Individual>).Clients.Add(newClient as Individual);
                }
                else
                {
                    newClient = new Juridical(AddClient.Name.Text, AddClient.Lastname.Text, AddClient.Patromymic.Text, ClientType.VIP, int.Parse(AddClient.Age.Text));
                    (DepItems[0] as Department<Juridical>).Clients.Add(newClient as Juridical);
                }
                AddClient.Close();
                JsonSerialize(this);
                MessageBox.Show("Клиент добавлен!");

            }); // добавление клиента
            EditClientButton = new Command(() =>
            {
                if (SelectedClient == null) { MessageBox.Show("Вы не выбрали клиента"); return; }
                EditClient = new EditClientWindow();
                EditClient.DataContext = this;
                EditClient.ShowDialog();
            }); // открытие окна изменения клиента
            EditClientButtonWindow = new Command(() =>
            {
                if (EditClient.Age.Text != "" && EditClient.Age.Text.Trim(' ') != "")
                {
                    if (int.Parse(EditClient.Age.Text) > 200) { MessageBox.Show("Вы ввели нереальный возраст"); return; }
                    SelectedClient.Age = int.Parse(EditClient.Age.Text);
                }
                if (EditClient.Name.Text != "" && EditClient.Name.Text.Trim(' ') != "") SelectedClient.FirstName = EditClient.Name.Text;
                if (EditClient.Lastname.Text != "" && EditClient.Lastname.Text.Trim(' ') != "") SelectedClient.LastName = EditClient.Lastname.Text;
                if (EditClient.Patronymic.Text != "" && EditClient.Patronymic.Text.Trim(' ') != "") SelectedClient.Patronymic = EditClient.Patronymic.Text;
                if (EditSelectedClientType != null)
                {
                    ClientType cltype = EditSelectedClientType.Tag.Equals("VIP") ? ClientType.VIP : EditSelectedClientType.Tag.Equals("Indiv") ? ClientType.Individual : ClientType.Juridical;
                    if (SelectedClient.ClientType == cltype) { EditClient.Close(); MessageBox.Show("Клиент изменен"); return; }
                    else
                    {
                        var c = SelectedClient;
                        switch (cltype)
                        {
                            case ClientType.VIP:
                                (DepItems[2] as Department<VIPClient>).Clients.Add(new VIPClient(c.FirstName, c.LastName, c.Patronymic, ClientType.VIP, c.Age)
                                { BankBalance = c.BankBalance, Investment = c.Investment });
                                break;
                            case ClientType.Individual:
                                (DepItems[1] as Department<Individual>).Clients.Add(new Individual(c.FirstName, c.LastName, c.Patronymic, ClientType.Individual, c.Age)
                                { BankBalance = c.BankBalance, Investment = c.Investment });
                                break;
                            case ClientType.Juridical:
                                (DepItems[0] as Department<Juridical>).Clients.Add(new Juridical(c.FirstName, c.LastName, c.Patronymic, ClientType.Juridical, c.Age)
                                { BankBalance = c.BankBalance, Investment = c.Investment });
                                break;
                            default:
                                break;
                        }
                        switch (c.ClientType)
                        {
                            case ClientType.VIP:
                                (DepItems[2] as Department<VIPClient>).Clients.Remove(SelectedClient as VIPClient);
                                SelectedClient = null;
                                break;
                            case ClientType.Individual:
                                (DepItems[1] as Department<Individual>).Clients.Remove(SelectedClient as Individual);
                                SelectedClient = null;
                                break;
                            case ClientType.Juridical:
                                (DepItems[0] as Department<Juridical>).Clients.Remove(SelectedClient as Juridical);
                                SelectedClient = null;
                                break;
                            default:
                                break;
                        }
                        EditClient.Close();
                        JsonSerialize(this);
                        MessageBox.Show("Клиент изменен");
                        return;
                    }
                }
                else { EditClient.Close(); MessageBox.Show("Клиент изменен"); return; }
            }); // изменение клиента
            DeleteClient = new Command(() =>
            {
                if (SelectedClient == null) { MessageBox.Show("Клиент не выбран"); return; }
                var res = MessageBox.Show("Вы уверены, что хотите удалить клиента?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No) return;
                switch (SelectedClient.ClientType)
                {
                    case ClientType.VIP:
                        (DepItems[2] as Department<VIPClient>).Clients.Remove(SelectedClient as VIPClient);
                        SelectedClient = null;
                        break;
                    case ClientType.Individual:
                        (DepItems[1] as Department<Individual>).Clients.Remove(SelectedClient as Individual);
                        SelectedClient = null;
                        break;
                    case ClientType.Juridical:
                        (DepItems[0] as Department<Juridical>).Clients.Remove(SelectedClient as Juridical);
                        SelectedClient = null;
                        break;
                    default:
                        break;
                }
                JsonSerialize(this);
                MessageBox.Show("Клиент удален");
            });
            NameClick = new Command(() =>
            {
                if (SelectedDepartment is Department<VIPClient>)
                {
                    var c = (DepItems[2] as Department<VIPClient>).Clients;
                    if (!namedesc)
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderBy(x => x.FirstName));
                        namedesc = true;
                    }
                    else
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderByDescending(x => x.FirstName));
                        namedesc = false;
                    }
                }
                else if (SelectedDepartment is Department<Individual>)
                {
                    var c = (DepItems[1] as Department<Individual>).Clients;
                    if (!namedesc)
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderBy(x => x.FirstName));
                        namedesc = true;
                    }
                    else
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderByDescending(x => x.FirstName));
                        namedesc = false;

                    }
                }
                else
                {
                    var c = (DepItems[0] as Department<Juridical>).Clients;
                    if (!namedesc)
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderBy(x => x.FirstName));
                        namedesc = true;
                    }
                    else
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderByDescending(x => x.FirstName));
                        namedesc = false;

                    }
                }
                
            });
            LastClick = new Command(() =>
            {
                if (SelectedDepartment is Department<VIPClient>)
                {
                    var c = (DepItems[2] as Department<VIPClient>).Clients;
                    if (!lastdesc)
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderBy(x => x.LastName));
                        lastdesc = true;
                    }
                    else
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderByDescending(x => x.LastName));
                        lastdesc = false;
                    }
                }
                else if (SelectedDepartment is Department<Individual>)
                {
                    var c = (DepItems[1] as Department<Individual>).Clients;
                    if (!lastdesc)
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderBy(x => x.LastName));
                        lastdesc = true;
                    }
                    else
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderByDescending(x => x.LastName));
                        lastdesc = false;

                    }
                }
                else
                {
                    var c = (DepItems[0] as Department<Juridical>).Clients;
                    if (!lastdesc)
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderBy(x => x.LastName));
                        lastdesc = true;
                    }
                    else
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderByDescending(x => x.LastName));
                        lastdesc = false;

                    }
                }
            });
            PatrClick = new Command(() =>
            {
                if (SelectedDepartment is Department<VIPClient>)
                {
                    var c = (DepItems[2] as Department<VIPClient>).Clients;
                    if (!agedesc)
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderBy(x => x.Age));
                        agedesc = true;
                    }
                    else
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderByDescending(x => x.Age));
                        agedesc = false;
                    }
                }
                else if (SelectedDepartment is Department<Individual>)
                {
                    var c = (DepItems[1] as Department<Individual>).Clients;
                    if (!agedesc)
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderBy(x => x.Age));
                        agedesc = true;
                    }
                    else
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderByDescending(x => x.Age));
                        agedesc = false;

                    }
                }
                else
                {
                    var c = (DepItems[0] as Department<Juridical>).Clients;
                    if (!agedesc)
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderBy(x => x.Age));
                        agedesc = true;
                    }
                    else
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderByDescending(x => x.Age));
                        agedesc = false;

                    }
                }
            });
            BalanceClick = new Command(() =>
            {
                if (SelectedDepartment is Department<VIPClient>)
                {
                    var c = (DepItems[2] as Department<VIPClient>).Clients;
                    if (!balancedesc)
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderBy(x => x.BankBalance));
                        balancedesc = true;
                    }
                    else
                    {
                        (DepItems[2] as Department<VIPClient>).Clients = new ObservableCollection<VIPClient>(c.OrderByDescending(x => x.BankBalance));
                        balancedesc = false;
                    }
                }
                else if (SelectedDepartment is Department<Individual>)
                {
                    var c = (DepItems[1] as Department<Individual>).Clients;
                    if (!balancedesc)
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderBy(x => x.BankBalance));
                        balancedesc = true;
                    }
                    else
                    {
                        (DepItems[1] as Department<Individual>).Clients = new ObservableCollection<Individual>(c.OrderByDescending(x => x.BankBalance));
                        balancedesc = false;

                    }
                }
                else
                {
                    var c = (DepItems[0] as Department<Juridical>).Clients;
                    if (!balancedesc)
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderBy(x => x.BankBalance));
                        balancedesc = true;
                    }
                    else
                    {
                        (DepItems[0] as Department<Juridical>).Clients = new ObservableCollection<Juridical>(c.OrderByDescending(x => x.BankBalance));
                        balancedesc = false;

                    }
                }
            });
            TransferInfo = new Command(obj =>
            {

            });
            #endregion

        }
        #endregion

        #region Рандомное заполнение

        private string ClientRep(int type)
        {
            if (type == 0)
                switch (Random.Next(0, 31))
                {
                    case 0: return "Филипп";
                    case 1: return "Харитон";
                    case 2: return "Корнелий";
                    case 3: return "Валерий";
                    case 4: return "Евгений";
                    case 5: return "Чарльз";
                    case 6: return "Оливер";
                    case 7: return "Цицерон";
                    case 8: return "Ананий";
                    case 9: return "Болеслав";
                    case 10: return "Пётр";
                    case 11: return "Яков";
                    case 12: return "Борис";
                    case 13: return "Зураб";
                    case 14: return "Яромир";
                    case 15: return "Закир";
                    case 16: return "Сава";
                    case 17: return "Никодим";
                    case 18: return "Эдуард";
                    case 19: return "Константин";
                    case 20: return "Трофим";
                    case 21: return "Орландо";
                    case 22: return "Бронислав";
                    case 23: return "Йозеф";
                    case 24: return "Вячеслав";
                    case 25: return "Борис";
                    case 26: return "Тимофей";
                    case 27: return "Богдан";
                    case 28: return "Филипп";
                    case 29: return "Феликс";
                    default:
                        return "Игнатий";
                }
            else if (type == 1)
                switch (Random.Next(0, 31))
                {
                    case 0: return "Чернов";
                    case 1: return "Носков";
                    case 2: return "Кулишенко";
                    case 3: return "Баранов";
                    case 4: return "Тарасюк";
                    case 5: return "Ершов";
                    case 6: return "Костин";
                    case 7: return "Некрасов";
                    case 8: return "Батейко";
                    case 9: return "Дзюба";
                    case 10: return "Денисов";
                    case 11: return "Давыдов";
                    case 12: return "Котовский";
                    case 13: return "Тихонов";
                    case 14: return "Волков";
                    case 15: return "Пархоменко";
                    case 16: return "Соловьёв";
                    case 17: return "Сафонов";
                    case 18: return "Марков";
                    case 19: return "Яловой";
                    case 20: return "Мамонтов";
                    case 21: return "Шарапов";
                    case 22: return "Блохин";
                    case 23: return "Мартынов";
                    case 24: return "Кошелев";
                    case 25: return "Евсеев";
                    case 26: return "Пархоменко";
                    case 27: return "Рожков";
                    case 28: return "Ковалё";
                    case 29: return "Третьяков";
                    default:
                        return "Гамула";
                }
            else if (type == 2)
                switch (Random.Next(0, 31))
                {
                    case 0: return "Алексеевич";
                    case 1: return "Андреевич";
                    case 2: return "Вадимович";
                    case 3: return "Григорьевич";
                    case 4: return "Романович";
                    case 5: return "Платонович";
                    case 6: return "Иванович";
                    case 7: return "Петрович";
                    case 8: return "Валериевич";
                    case 9:  return "Фёдорович";
                    case 10: return "Романович";
                    case 11: return "Львович";
                    case 12: return "Ярославович";
                    case 13: return "Романович";
                    case 14: return "Богданович";
                    case 15: return "Леонидович";
                    case 16: return "Викторович";
                    case 17: return "Станиславович";
                    case 18: return "Брониславович";
                    case 19: return "Юхимович";
                    case 20: return "Евгеньевич";
                    case 21: return "Сергеевич";
                    case 22: return "Данилович";
                    case 23: return "Эдуардович";
                    case 24: return "Владимирович";
                    case 25: return "Анатолиевич";
                    case 26: return "Фёдорович";
                    case 27: return "Вадимович";
                    case 28: return "Дмитриевич";
                    case 29: return "Русланович";
                    case 30: return "Федотович";
                    default: return "Миронович";

                }
            return null;
        } // репозиторий ФИО
        private Investment RandomInvest(ClientType type)
        {
            switch (type)
            {
                case ClientType.VIP:
                    if (Random.Next(0, 2) == 0)
                        if (Random.Next(0, 2) == 0) return new Investment(InvestmentType.Capitalization, ClientType.VIP, Random.Next(500, 10000),
                            new DateTime(Random.Next(2010, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                        else return new Investment(InvestmentType.NotCapitalization, ClientType.VIP, Random.Next(500, 10000),
                            new DateTime(Random.Next(2010,DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                    else return null;
                case ClientType.Individual:
                    if (Random.Next(0, 2) == 0)
                        if (Random.Next(0, 2) == 0) return new Investment(InvestmentType.Capitalization, ClientType.Individual, Random.Next(500, 10000),
                            new DateTime(Random.Next(2010, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                        else return new Investment(InvestmentType.NotCapitalization, ClientType.Individual, Random.Next(500, 10000),
                            new DateTime(Random.Next(2010, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                    else return null;
                case ClientType.Juridical:
                    if (Random.Next(0, 2) == 0)
                        if (Random.Next(0, 2) == 0) return new Investment(InvestmentType.Capitalization, ClientType.Juridical, Random.Next(500, 10000),
                            new DateTime(Random.Next(2010, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                        else return new Investment(InvestmentType.NotCapitalization, ClientType.Juridical, Random.Next(500, 10000),
                            new DateTime(Random.Next(2010, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                    else return null;
                default:
                    break;
            }
            return null;
        } // рандомная информация о вкладе
        public static string CardRandom()
        {
            string longrandom = Random.Next(1_000_000_000, int.MaxValue).ToString() + Random.Next(1_000_000_000, int.MaxValue).ToString();
            longrandom = longrandom.Substring(longrandom.Length - 16);
            if (longrandom[0] == '0')
            {
                int a = longrandom.Length;
                longrandom = longrandom.TrimStart('0');
                while(a != longrandom.Length)
                {
                    longrandom += Random.Next(1, 10).ToString();
                }
                return longrandom;
            }
            return longrandom;
        }
        public void FillClients(int count)
        {
            for (int i = 0; i < count; i++)
            {
                (DepItems[0] as Department<Juridical>).Clients.Add(new Juridical(ClientRep(0), ClientRep(1), ClientRep(2), ClientType.Juridical, Random.Next(18, 40))
                { Investment = RandomInvest(ClientType.VIP), BankBalance = Random.Next(10, 100000), CardNumber = long.Parse(CardRandom()) });
            }
            for (int i = 0; i < count; i++)
            {
               (DepItems[1] as Department<Individual>).Clients.Add(new Individual(ClientRep(0), ClientRep(1), ClientRep(2), ClientType.Individual, Random.Next(18, 40))
               { Investment = RandomInvest(ClientType.VIP), BankBalance = Random.Next(10, 100000), CardNumber = long.Parse(CardRandom()) });
            }
            for (int i = 0; i < count; i++)
            {
                (DepItems[2] as Department<VIPClient>).Clients.Add(new VIPClient(ClientRep(0), ClientRep(1), ClientRep(2), ClientType.VIP, Random.Next(18, 40))
                { Investment = RandomInvest(ClientType.VIP), BankBalance = Random.Next(10, 100000), CardNumber = long.Parse(CardRandom()) });
                
            }
        } // заполнение сотрудниками
        #endregion

        #region Сериализация

        private Department<Juridical> JuridJsonDeserialize()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string json = File.ReadAllText($@"{path}\BankLogs\Juridical.json");
            return JsonConvert.DeserializeObject<Department<Juridical>>(json);
        }
        private Department<Individual> IndivJsonDeserialize()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string json = File.ReadAllText($@"{path}\BankLogs\Individual.json");
            return JsonConvert.DeserializeObject<Department<Individual>>(json);
        }
        private Department<VIPClient> VIPJsonDeserialize()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string json = File.ReadAllText($@"{path}\BankLogs\VIP.json");
            return JsonConvert.DeserializeObject<Department<VIPClient>>(json);
        }
        private void JsonSerialize(Bank bank)
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
        #endregion

        private void ClickInfo() // выполняется при нажатии на кнопку информации
        {
            if (Info != null) return;
            if (SelectedClient == null) return;
            if (SelectedClient.Investment == null)
            {
                if (CreateInvest != null) return;
                var result = MessageBox.Show("У выбранного клиента нет вклада. Хотите создать?", "Информация", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                CreateInvest = new InvestCreateWindow();
                CreateInvest.DataContext = this;
                CreateInvest.Closed += (sender, e) => { this.CreateInvest = null; };
                CreateInvest.ShowDialog();
                return;
            }

            try
            {
                SelectedClient.Investment.CurrentDate = this.CurrentDate;
                Info = new ClientInfoWindow();
                Info.DataContext = SelectedClient;
                Info.Closed += (sender, e) => { this.Info = null; };
                Info.WithDraw.DataContext = this;
                Info.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }


        }



        private bool CheckCardNumber(long cardnumber, out AbstractClient client)
        {
            foreach (var item in (DepItems[0] as Department<Juridical>).Clients)
                if (item.CardNumber == cardnumber) { client = item; return true; }
            foreach (var item in (DepItems[1] as Department<Individual>).Clients)
                if (item.CardNumber == cardnumber) { client = item; return true; }
            foreach (var item in (DepItems[2] as Department<VIPClient>).Clients)
                if (item.CardNumber == cardnumber) { client = item; return true; }
            client = null;
            return false;
        } // проверка на существование клиента, и возвращение его
    }
}
