using ComandLib;
using InvestmentLib;
using MyExceptionsLib;
using Newtonsoft.Json;
using PropertiesChangedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TransactionLib;
using static EnumsLib.Enums;

namespace BankingSystem
{

    public class Bank : PropertiesChanged // Модель представления банка для основногo окна
    {
        #region Объект блокировки

        object o = new object();
        #endregion

        #region Базы данных

        public ClientsDataBase Clients { get; set; }

        #endregion

        #region События
        public event Action<TransactionInfo> OnPayment;
        public event Action<TransactionInfo> OnReceive;
        #endregion

        #region Рандом
        public static Random Random = new Random(123);
        public static Random JurRandom = new Random(323);
        public static Random IndRandom = new Random(2443);
        public static Random VipRandom = new Random(2343);
        #endregion

        #region Окна
        private ClientInfoWindow Info { get; set; } // окно с информацией 
        private InvestCreateWindow CreateInvest { get; set; }
        private TransferWindow Transfer { get; set; }
        private DepositWindow Deposit { get; set; }
        private AddClientWindow AddClient { get; set; }
        private EditClientWindow EditClient { get; set; }
        private TransactionInfoWindow TransactionInfo { get; set; }
        public LoadingScreen Load { get; set; }
        #endregion

        #region Переданные данные через View
        public AbstractClient SelectedClient { get; set; } // Информация о выбранном клиенте, которая передается через View
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public ComboBoxItem SelectedInvType { get; set; }
        public ListView ListOfClients { get; set; }
        public ComboBoxItem SelectedClientType { get; set; }
        public ComboBoxItem EditSelectedClientType { get; set; }

        //сделай датаконтекст листвью здесь, чтобы он заполнялся от выбранного отдела
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
        public ICommand ChangeDep { get; set; }

        #endregion

        #region Дополнительные переменные для сортировки
        bool namedesc = false;
        bool lastdesc = false;
        bool patrdesc = false;
        bool balancedesc = false;
        #endregion

        #region Конструктор 
        public Bank()
        {
            #region DB init

            #region JuridCommands
            var selectJur = @"SELECT * FROM JuridicalClients Order By JuridicalClients.id ";
            var deleteJur = @"DELETE FROM JuridicalClients WHERE id = @id";

            var insertJur = @"INSERT INTO JuridicalClients (clientName, clientLastname, clientPatronymic,
                                                clientAge, cardNumber, bankBalance)
                                        VALUES (@clientName, @clientLastname, @clientPatronymic,
                                                @clientAge, @cardNumber, @bankBalance)";

            var updateJur = @"UPDATE JuridicalClients SET clientName = @clientName, clientLastname = @clientLastname,
                                              clientPatronymic = @clientPatronymic,
                                              clientAge = @clientAge,
                                              cardNumber = @cardNumber,
                                              bankBalance = @bankBalance,
                          WHERE id = @id";

            #endregion


            Clients = new ClientsDataBase(selectJur, insertJur, updateJur, deleteJur);
            #endregion

            FillClients(10);

            #region Команды




            //InfoClick = new Command(ClickInfo); // открытие информационной панели о вкладе
            //WithdrawButton = new Command(() =>
            //{
            //    var res = MessageBox.Show($"Вы уверены что хотите вывести вклад? Вы выведите {SelectedClient.Investment.CurrentSum}$", "",
            //        MessageBoxButton.YesNo, MessageBoxImage.Question);
            //    if (res != MessageBoxResult.Yes) return;
            //    SelectedClient.BankBalance += SelectedClient.Investment.CurrentSum;
            //    SelectedClient.Investment = null;
            //    Info.Close();
            //    this.JsonSerializer();

            //}); // вывод средств из вклада на счет
            //InvestmentButton = new Command(() =>
            //{
            //    #region Обратные условия
            //    if (string.IsNullOrEmpty(CreateInvest.InvestField.Text)) { MessageBox.Show("Ничего не введено"); return; }
            //    if (!long.TryParse(CreateInvest.InvestField.Text, out long result)) { MessageBox.Show("Введена строка или слишком большое число"); return; }
            //    if (SelectedClient.BankBalance < result) { MessageBox.Show($"Недостаточно средств для вклада. Ваши средства: {SelectedClient.BankBalance}$"); return; }
            //    if (result < 500) { MessageBox.Show($"Минимальное количество средств для вклада - 500$. Ваши средства: {SelectedClient.BankBalance}$"); return; }
            //    if (SelectedInvType == null) { MessageBox.Show("Вы не выбрали тип вклада"); return; }

            //    #endregion

            //    InvestmentType type = (string)SelectedInvType.Content == "С капитализацией" ? InvestmentType.Capitalization : InvestmentType.NotCapitalization;
            //    ClientType clientType = SelectedClient.GetType() == typeof(Individual) ? ClientType.Individual : SelectedClient.GetType() == typeof(VIPClient) ? ClientType.VIP : ClientType.Juridical;
            //    SelectedClient.Investment = new Investment(type, clientType, result, CurrentDate);
            //    SelectedClient.BankBalance = SelectedClient.BankBalance - result;
            //    CreateInvest.Close();
            //    this.JsonSerializer();
            //}); // окно открытия вклада
            //TransferButton = new Command(() =>
            //{
            //    if (this.Transfer != null) return;
            //    if (SelectedClient == null) return;
            //    if (SelectedClient.BankBalance < 10) { MessageBox.Show("Недостаточно денег для перевода. Минимальное количество - 10$"); return; }
            //    Transfer = new TransferWindow();
            //    Transfer.Closed += (sender, e) => { this.Transfer = null; };
            //    Transfer.DataContext = this;
            //    Transfer.ShowDialog();
            //}); // открытие окна перевода средств
            //TransferButtonWindow = new Command(() =>
            //{
            //    var enumcard = Transfer.CardNumber.Text.Where(x => x != ' '); // удаляю лишние пробелы (если такие есть)
            //    string card = enumcard.Aggregate<char, string>(null, (current, item) => current + item);

            //    #region Обратные условия

            //    if (string.IsNullOrEmpty(card)) { MessageBox.Show("Поле пустое"); return; }
            //    if (!long.TryParse(card, out long result)) { MessageBox.Show("В поле для ввода карты введена строка или слишком большое число"); return; }
            //    if (!CheckCardNumber(result, out AbstractClient client)) { MessageBox.Show("Такой карты не существует"); return; }
            //    if (!long.TryParse(Transfer.TransferSum.Text, out long sum)) { MessageBox.Show("В поле для ввода суммы введена строка или слишком большое число"); return; }
            //    if (SelectedClient.BankBalance < sum) { MessageBox.Show($"Недостаточно средств для перевода. Ваши средства: {SelectedClient.BankBalance}$"); return; }

            //    #endregion
            //    MessageBoxResult res;
            //    long newsum = 0;
            //    OnPayment += SelectedClient.Transaction;
            //    OnReceive += client.Transaction;
            //    switch (SelectedClient.ClientType)
            //    {
            //        case ClientType.VIP:
            //            newsum = sum;
            //            res = MessageBox.Show($"Комиссия перевода - 0%. Вы переведете клиенту {newsum}$", "Информация", MessageBoxButton.YesNo);
            //            if (res != MessageBoxResult.Yes) return;
            //            SelectedClient.BankBalance -= newsum;
            //            client.BankBalance += newsum;
            //            break;
            //        case ClientType.Individual:
            //            newsum = sum + (sum / 100 * 3);
            //            res = MessageBox.Show($"Комиссия перевода - 3%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
            //            if (res != MessageBoxResult.Yes) return;
            //            SelectedClient.BankBalance -= newsum;
            //            client.BankBalance += sum;
            //            break;
            //        case ClientType.Juridical:
            //            newsum = sum + (sum / 100 * 2);
            //            res = MessageBox.Show($"Комиссия перевода - 2%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
            //            if (res != MessageBoxResult.Yes) return;
            //            SelectedClient.BankBalance -= newsum;
            //            client.BankBalance += sum;
            //            break;
            //        default:
            //            break;
            //    }
            //    Transfer.Close();
            //    var c = SelectedClient;
            //    var payment = new TransactionInfo(client.FirstName, client.LastName, client.Patronymic, client.CardNumber,
            //        -newsum, client.ClientType) { Type = TransactionType.Payment };
            //    var receiver = new TransactionInfo(c.FirstName, c.LastName, c.Patronymic, c.CardNumber,
            //        sum, c.ClientType) { Type = TransactionType.Receive };
            //    OnPayment?.Invoke(payment);
            //    OnReceive?.Invoke(receiver);
            //    OnPayment -= SelectedClient.Transaction;
            //    OnReceive -= client.Transaction;
            //    this.JsonSerializer();
            //}); // перевод средств на другой счет
            //DepositButton = new Command(() =>
            //{
            //    if (SelectedClient == null) return;
            //    Deposit = new DepositWindow();
            //    Deposit.DataContext = this;
            //    Deposit.Closed += (sender, e) => { this.Deposit = null; };
            //    Deposit.ShowDialog();
            //}); // открытие окна пополнения счета
            //DepositButtonWindow = new Command(() =>
            //{
            //    if (string.IsNullOrEmpty(Deposit.DepositField.Text)) { MessageBox.Show("Поле пустое"); return; }
            //    if (!long.TryParse(Deposit.DepositField.Text, out long result)) { MessageBox.Show("В поле ввода строка или число слишком большое"); return; }
            //    try
            //    {
            //        if (result > 10000) throw new DepositException("Нельзя пополнить счет на сумму более 10000$ за раз");
            //    }
            //    catch (DepositException e)
            //    {
            //        MessageBox.Show(e.Message);
            //        return;
            //    }
            //    SelectedClient.BankBalance += result;
            //    Deposit.Close();
            //    this.JsonSerializer();
            //}); // пополнение счета 
            //AddClientButton = new Command(() =>
            //{
            //    AddClient = new AddClientWindow();
            //    AddClient.DataContext = this;
            //    AddClient.ShowDialog();

            //}); // открытие окна добавления клиента
            //AddClientButtonWindow = new Command(() =>
            //{
            //    if (string.IsNullOrEmpty(AddClient.Name.Text) || string.IsNullOrEmpty(AddClient.Lastname.Text) || string.IsNullOrEmpty(AddClient.Patromymic.Text)
            //        || string.IsNullOrEmpty(AddClient.Age.Text) || AddClient.Name.Text.Trim(' ') == "" || AddClient.Lastname.Text.Trim(' ') == "" ||
            //            AddClient.Patromymic.Text.Trim(' ') == "" || AddClient.Age.Text.Trim(' ') == "") { MessageBox.Show("Одно или несколько полей не введены"); return; }

            //    if (!int.TryParse(AddClient.Age.Text, out int res)) { MessageBox.Show("Введен неправильный возраст"); return; }
            //    if (res > 200) { MessageBox.Show("Введен невозможный для человека возраст"); return; }
            //    if (SelectedClientType == null) { MessageBox.Show("Вы не выбрали тип клиента"); return; }
            //    AbstractClient newClient = null;
            //    if (SelectedClientType.Tag.Equals("VIP"))
            //    {
            //        newClient = new VIPClient(AddClient.Name.Text, AddClient.Lastname.Text, AddClient.Patromymic.Text, ClientType.VIP, int.Parse(AddClient.Age.Text))
            //        { CardNumber = long.Parse(CardRandom(VipRandom)) };
            //        newClient.AddTo(DepItems[2] as Department<VIPClient>);
            //    }
            //    else if (SelectedClientType.Tag.Equals("Indiv"))
            //    {
            //        newClient = new Individual(AddClient.Name.Text, AddClient.Lastname.Text, AddClient.Name.Text, ClientType.VIP, int.Parse(AddClient.Age.Text))
            //        { CardNumber = long.Parse(CardRandom(IndRandom)) };
            //        newClient.AddTo(DepItems[1] as Department<Individual>);
            //    }
            //    else
            //    {
            //        newClient = new Juridical(AddClient.Name.Text, AddClient.Lastname.Text, AddClient.Patromymic.Text, ClientType.VIP, int.Parse(AddClient.Age.Text))
            //        { CardNumber = long.Parse(CardRandom(JurRandom)) };
            //        newClient.AddTo(DepItems[0] as Department<Juridical>);
            //    }
            //    AddClient.Close();
            //    this.JsonSerializer();

            //}); // добавление клиента
            //EditClientButton = new Command(() =>
            //{
            //    if (SelectedClient == null) { MessageBox.Show("Вы не выбрали клиента"); return; }
            //    EditClient = new EditClientWindow();
            //    EditClient.DataContext = this;
            //    EditClient.ShowDialog();
            //}); // открытие окна изменения клиента
            //EditClientButtonWindow = new Command(() =>
            //{
            //    if (EditClient.Age.Text != "" && EditClient.Age.Text.Trim(' ') != "")
            //    {
            //        if (int.Parse(EditClient.Age.Text) > 200) { MessageBox.Show("Вы ввели нереальный возраст"); return; }
            //        SelectedClient.Age = int.Parse(EditClient.Age.Text);
            //    }
            //    if (EditClient.Name.Text != "" && EditClient.Name.Text.Trim(' ') != "") SelectedClient.FirstName = EditClient.Name.Text;
            //    if (EditClient.Lastname.Text != "" && EditClient.Lastname.Text.Trim(' ') != "") SelectedClient.LastName = EditClient.Lastname.Text;
            //    if (EditClient.Patronymic.Text != "" && EditClient.Patronymic.Text.Trim(' ') != "") SelectedClient.Patronymic = EditClient.Patronymic.Text;
            //    if (EditSelectedClientType != null)
            //    {
            //        ClientType cltype = EditSelectedClientType.Tag.Equals("VIP") ? ClientType.VIP : EditSelectedClientType.Tag.Equals("Indiv") ? ClientType.Individual : ClientType.Juridical;
            //        if (SelectedClient.ClientType == cltype) { EditClient.Close(); this.JsonSerializer(); return; }
            //        else
            //        {
            //            var c = SelectedClient;
            //            switch (cltype)
            //            {
            //                case ClientType.VIP:

            //                    var vip = new VIPClient(c.FirstName, c.LastName, c.Patronymic, ClientType.VIP, c.Age)
            //                    { BankBalance = c.BankBalance, Investment = c.Investment, CardNumber = c.CardNumber };
            //                    vip.AddTo(DepItems[2] as Department<VIPClient>);
            //                    break;
            //                case ClientType.Individual:
            //                    var ind = new Individual(c.FirstName, c.LastName, c.Patronymic, ClientType.Individual, c.Age)
            //                    { BankBalance = c.BankBalance, Investment = c.Investment, CardNumber = c.CardNumber };
            //                    ind.AddTo(DepItems[1] as Department<Individual>);
            //                    break;
            //                case ClientType.Juridical:
            //                    var jur = new Juridical(c.FirstName, c.LastName, c.Patronymic, ClientType.Juridical, c.Age)
            //                    { BankBalance = c.BankBalance, Investment = c.Investment, CardNumber = c.CardNumber };
            //                    jur.AddTo(DepItems[0] as Department<Juridical>);
            //                    break;
            //                default:
            //                    break;
            //            }
            //            switch (c.ClientType)
            //            {
            //                case ClientType.VIP:
            //                    SelectedClient.RemoveFrom(DepItems[2] as Department<VIPClient>);
            //                    SelectedClient = null;
            //                    break;
            //                case ClientType.Individual:
            //                    SelectedClient.RemoveFrom(DepItems[1] as Department<Individual>);
            //                    SelectedClient = null;
            //                    break;
            //                case ClientType.Juridical:
            //                    SelectedClient.RemoveFrom(DepItems[0] as Department<Juridical>);
            //                    SelectedClient = null;
            //                    break;
            //                default:
            //                    break;
            //            }
            //            EditClient.Close();
            //            this.JsonSerializer();

            //        }
            //    }
            //    else { EditClient.Close(); this.JsonSerializer(); return; }
            //}); // изменение клиента
            //DeleteClient = new Command(() =>
            //{
            //    if (SelectedClient == null) { MessageBox.Show("Клиент не выбран"); return; }
            //    var res = MessageBox.Show("Вы уверены, что хотите удалить клиента?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //    if (res == MessageBoxResult.No) return;
            //    switch (SelectedClient.ClientType)
            //    {
            //        case ClientType.VIP:
            //            (DepItems[2] as Department<VIPClient>).Clients.Remove(SelectedClient as VIPClient);
            //            SelectedClient = null;
            //            break;
            //        case ClientType.Individual:
            //            (DepItems[1] as Department<Individual>).Clients.Remove(SelectedClient as Individual);
            //            SelectedClient = null;
            //            break;
            //        case ClientType.Juridical:
            //            (DepItems[0] as Department<Juridical>).Clients.Remove(SelectedClient as Juridical);
            //            SelectedClient = null;
            //            break;
            //        default:
            //            break;
            //    }
            //    this.JsonSerializer();
            //});
            //NameClick = new Command(() =>
            //{
            //    Load = new LoadingScreen();
            //    Load.Show();
            //    lastdesc = false;
            //    patrdesc = false;
            //    balancedesc = false;
            //    Thread th = new Thread(() =>
            //    {
            //        if (SelectedDepartment is Department<VIPClient>)
            //        {
            //            var c = (DepItems[2] as Department<VIPClient>).Clients;
            //            if (!namedesc)
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.FirstName, true);
            //                namedesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.FirstName, false);
            //                namedesc = false;
            //            }
            //        }
            //        else if (SelectedDepartment is Department<Individual>)
            //        {
            //            var c = (DepItems[1] as Department<Individual>).Clients;
            //            if (!namedesc)
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.FirstName, true);
            //                namedesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.FirstName, false);
            //                namedesc = false;
            //            }
            //        }
            //        else
            //        {
            //            var c = (DepItems[0] as Department<Juridical>).Clients;
            //            if (!namedesc)
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.FirstName, true);
            //                namedesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.FirstName, false);
            //                namedesc = false;

            //            }
            //        }
            //        Application.Current.Dispatcher.Invoke(() => Load.Close());
            //    });
            //    th.Start();
            //});
            //LastClick = new Command(() =>
            //{
            //    Load = new LoadingScreen();
            //    Load.Show();
            //    namedesc = false;
            //    patrdesc = false;
            //    balancedesc = false;
            //    var th = new Thread(() =>
            //    {
            //        if (SelectedDepartment is Department<VIPClient>)
            //        {
            //            var c = (DepItems[2] as Department<VIPClient>).Clients;
            //            if (!lastdesc)
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.LastName, true);
            //                lastdesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.LastName, false);
            //                lastdesc = false;
            //            }
            //        }
            //        else if (SelectedDepartment is Department<Individual>)
            //        {
            //            var c = (DepItems[1] as Department<Individual>).Clients;
            //            if (!lastdesc)
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.LastName, true);
            //                lastdesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.LastName, false);
            //                namedesc = false;

            //            }
            //        }
            //        else
            //        {
            //            var c = (DepItems[0] as Department<Juridical>).Clients;
            //            if (!lastdesc)
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.LastName, true);
            //                lastdesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.LastName, false);
            //                lastdesc = false;

            //            }
            //        }
            //        Application.Current.Dispatcher.Invoke(() => Load.Close());
            //    });
            //    th.Start();
            //});
            //PatrClick = new Command(() =>
            //{
            //    Load = new LoadingScreen();
            //    Load.Show();
            //    namedesc = false;
            //    lastdesc = false;
            //    balancedesc = false;
            //    Thread th = new Thread(() =>
            //    {
            //        if (SelectedDepartment is Department<VIPClient>)
            //        {
            //            var c = (DepItems[2] as Department<VIPClient>).Clients;
            //            if (!patrdesc)
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.Patronymic, true);
            //                patrdesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.Patronymic, false);
            //                patrdesc = false;
            //            }
            //        }
            //        else if (SelectedDepartment is Department<Individual>)
            //        {
            //            var c = (DepItems[1] as Department<Individual>).Clients;
            //            if (!patrdesc)
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.Patronymic, true);
            //                patrdesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.Patronymic, false);
            //                patrdesc = false;

            //            }
            //        }
            //        else
            //        {
            //            var c = (DepItems[0] as Department<Juridical>).Clients;
            //            if (!patrdesc)
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.Patronymic, true);
            //                patrdesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.Patronymic, false);
            //                patrdesc = false;

            //            }
            //        }
            //        Application.Current.Dispatcher.Invoke(() => Load.Close());
            //    });
            //    th.Start();
            //});
            //BalanceClick = new Command(() =>
            //{
            //    Load = new LoadingScreen();
            //    Load.Show();
            //    namedesc = false;
            //    lastdesc = false;
            //    patrdesc = false;
            //    Thread th = new Thread(() =>
            //    {
            //        if (SelectedDepartment is Department<VIPClient>)
            //        {
            //            var c = (DepItems[2] as Department<VIPClient>).Clients;
            //            if (!balancedesc)
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.BankBalance, true);
            //                balancedesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[2] as Department<VIPClient>).Order(x => x.BankBalance, false);
            //                balancedesc = false;
            //            }
            //        }
            //        else if (SelectedDepartment is Department<Individual>)
            //        {
            //            var c = (DepItems[1] as Department<Individual>).Clients;
            //            if (!balancedesc)
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.BankBalance, true);
            //                balancedesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[1] as Department<Individual>).Order(x => x.BankBalance, false);
            //                balancedesc = false;

            //            }
            //        }
            //        else
            //        {
            //            var c = (DepItems[0] as Department<Juridical>).Clients;
            //            if (!balancedesc)
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.BankBalance, true);
            //                balancedesc = true;
            //            }
            //            else
            //            {
            //                (DepItems[0] as Department<Juridical>).Order(x => x.BankBalance, false);
            //                balancedesc = false;

            //            }
            //        }
            //        Application.Current.Dispatcher.Invoke(() => Load.Close());
            //    });
            //    th.Start();
            //});
            //TransferInfo = new Command(e =>
            //{
            //    var client = e as AbstractClient;
            //    TransactionInfo = new TransactionInfoWindow();
            //    TransactionInfo.DataContext = client;
            //    TransactionInfo.Show();
            //});
            #endregion

        }
        #endregion

        #region Рандомное заполнение

        private string ClientRep(int type, Random random)
        {
            if (type == 0)
                switch (random.Next(0, 31))
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
                switch (random.Next(0, 31))
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
                switch (random.Next(0, 31))
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
        //private Investment RandomInvest(ClientType type)
        //{

        //}
        public static string CardRandom(Random random)
        {
            string longrandom = random.Next(1_000_000_000, int.MaxValue).ToString() + random.Next(1_000_000_000, int.MaxValue).ToString();
            longrandom = longrandom.Substring(longrandom.Length - 16);
            if (longrandom[0] == '0')
            {
                int a = longrandom.Length;
                longrandom = longrandom.TrimStart('0');
                while(a != longrandom.Length)
                {
                    longrandom += random.Next(1, 10).ToString();
                }
                return longrandom;
            }
            return longrandom;
        }
        public void FillClients(int count)
        {
            if (Clients.Table.Rows.Count != 0 && Clients.Table.Rows.Count != 0
                && Clients.Table.Rows.Count != 0) return;

            for (int i = 0; i < count; i++)
            {
                DataRow data = Clients.Table.NewRow();
                data[1] = ClientRep(0, JurRandom);
                data[2] = ClientRep(1, JurRandom);
                data[3] = ClientRep(2, JurRandom);
                data[4] = Random.Next(18, 31);
                data[5] = Random.Next(18, 31);
                data[6] = Random.Next(10, 200000);
                Clients.Table.Rows.Add(data);
            }

            for (int i = 0; i < count; i++)
            {
                DataRow data = Clients.Table.NewRow();
                data[1] = ClientRep(0, IndRandom);
                data[2] = ClientRep(1, IndRandom);
                data[3] = ClientRep(2, IndRandom);
                data[4] = Random.Next(18, 31);
                data[5] = Random.Next(18, 31);
                data[6] = Random.Next(10, 200000);
                Clients.Table.Rows.Add(data);
            }

            for (int i = 0; i < count; i++)
            {
                DataRow data = Clients.Table.NewRow();
                data[1] = ClientRep(0, VipRandom);
                data[2] = ClientRep(1, VipRandom);
                data[3] = ClientRep(2, VipRandom);
                data[4] = Random.Next(18, 31);
                data[5] = Random.Next(18, 31);
                data[6] = Random.Next(10, 200000);
                Clients.Table.Rows.Add(data);
            }
            Clients.Adapter.Update(Clients.Table);
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
            client = new Juridical("1","1", "1", ClientType.Individual, 1);
            return false;

        } // проверка на существование клиента, и возвращение его
    }
}
