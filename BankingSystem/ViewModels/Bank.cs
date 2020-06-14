using BankingSystem.DataBase;
using ComandLib;
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

        #region Базы данных
        public ClientsDataBase Clients { get; set; }
        public InvestmentsDataBase Investments { get; set; }
        public TransactionsDataBase Transactions { get; set; }

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
        public DataRowView SelectedClient { get; set; }
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
        public ICommand CopyCardNumber { get; set; }

        #endregion

        #region Дополнительные поля
        bool namedesc = false;
        bool lastdesc = false;
        bool patrdesc = false;
        bool balancedesc = false;

        int currentSum = default;
        #endregion

        #region Свойства

        public DataRowView GetInvestmentFromClient
        {
            get
            {
                try
                {
                    return Investments.Table.DefaultView[Investments.Table.Rows.IndexOf(Investments.Table.AsEnumerable()
                                .First(x => x.Field<int>("clientId") == (int)SelectedClient[0]))];
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion

        #region Конструктор

        public Bank()
        {
            #region DB init

            #region ClientCommands
            var select = @"SELECT * FROM Clients Order By Clients.id ";
            var delete = @"DELETE FROM Clients WHERE id = @id";
            var insert = @"INSERT INTO Clients (clientName, clientLastname, clientPatronymic,
                                                clientAge, cardNumber, bankBalance, clientType)
                                        VALUES (@clientName, @clientLastname, @clientPatronymic,
                                                @clientAge, @cardNumber, @bankBalance, @clientType)
                                        SET @id = @@IDENTITY";
            var updateVip = @"UPDATE Clients SET clientName = @clientName, clientLastname = @clientLastname,
                                              clientPatronymic = @clientPatronymic,
                                              clientAge = @clientAge,
                                              cardNumber = @cardNumber,
                                              bankBalance = @bankBalance,
                                              clientType = @clientType
                          WHERE id = @id";
            #endregion

            #region Investments Commands

            var selectInv = @"SELECT * FROM Investments Order By Investments.Id ";
            var deleteInv = @"DELETE FROM Investments WHERE Id = @Id";
            var insertInv = @"INSERT INTO Investments (clientId, investmentType, investmentDate, investmentSum, percentage)
                                            VALUES (@clientId, @investmentType, @investmentDate,@investmentSum, @percentage)
                                            SET @Id = @@IDENTITY";
            var updateInv = @"UPDATE Investments SET clientId = @clientId,
                                                    investmentType = @investmentType
                                                    investmentDate = @investmentDate
                                                    investmentSum = @investmentSum
                                                    percentage = @percentage
                          WHERE Id = @Id";

            #endregion

            #region Transactions Commands

            var selectTrans = @"SELECT * FROM Transactions Order By Transactions.Id ";
            var deleteTrans = @"DELETE FROM Transactions WHERE Id = @Id";
            var insertTrans = @"INSERT INTO Transactions (ClientId, NameTarget, LastnameTarget,
                                                PatronymicTarget, CardTarget, ClientTypeTarget, TransactionSum, Type)
                                        VALUES (@ClientId, @NameTarget, @LastnameTarget,
                                                @PatronymicTarget, @CardTarget, @ClientTypeTarget, @TransactionSum, @Type)
                                        SET @Id = @@IDENTITY";
            var updateTrans = @"UPDATE Transactions SET ClientId = @ClientId, NameTarget = @NameTarget,
                                              LastnameTarget = @LastnameTarget,
                                              PatronymicTarget = @PatronymicTarget,
                                              CardTarget = @CardTarget,
                                              ClientTypeTarget = @ClientTypeTarget,
                                              TransactionSum = @TransactionSum,
                                              Type = @Type
                            WHERE Id = @Id";
            #endregion

            Clients = new ClientsDataBase(select, insert, updateVip, delete);
            Investments = new InvestmentsDataBase(selectInv, insertInv, updateInv, deleteInv);
            Transactions = new TransactionsDataBase(selectTrans, insertTrans, updateTrans, deleteTrans);
            #endregion

            FillClients(200000);

            #region Команды
            InfoClick = new Command(() => 
            {
                if (SelectedClient == null) return;
                if (GetInvestmentFromClient == null)
                {
                    var result = MessageBox.Show("У выбранного клиента нет вклада. Хотите создать?", "Информация", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;

                    CreateInvest = new InvestCreateWindow();
                    CreateInvest.DataContext = this;
                    CreateInvest.ShowDialog();
                    Investments.Update();
                    return;
                }
                try
                {
                    Info = new ClientInfoWindow
                    {
                        DataContext = GetInvestmentFromClient
                    };
                    Info.OnWithdraw += (e) =>
                    {
                        SelectedClient.Row["bankBalance"] = (int)SelectedClient.Row["bankBalance"] + e;
                        GetInvestmentFromClient.Row.Delete();

                        Investments.Update();
                        Clients.Update();
                        Info.Close();
                    };

                    Info.Closed += (sender, e) => { this.Info = null; };
                    Info.WithDraw.DataContext = this;
                    Info.CurrentDate.DataContext = CurrentDate.ToShortDateString();
                    Info.ShowDialog();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }

            });
            InvestmentButton = new Command(() =>
            {
                #region Обратные условия
                if (string.IsNullOrEmpty(CreateInvest.InvestField.Text)) { MessageBox.Show("Ничего не введено"); return; }

                if (!int.TryParse(CreateInvest.InvestField.Text, out int result)) { MessageBox.Show("Введена строка или слишком большое число"); return; }

                if ((int)SelectedClient.Row["bankBalance"] < result) { MessageBox.Show($"Недостаточно средств для вклада. Ваши средства:" +
                    $" {SelectedClient.Row["bankBalance"]}$"); return; }

                if (result < 500) { MessageBox.Show($"Минимальное количество средств для вклада - 500$. Ваши средства:" +
                    $" {SelectedClient.Row["bankBalance"]}$"); return; }

                if (SelectedInvType == null) { MessageBox.Show("Вы не выбрали тип вклада"); return; }

                #endregion

                InvestmentType type = (string)SelectedInvType.Content == "С капитализацией" ? InvestmentType.Capitalization : InvestmentType.NotCapitalization;
                DataRow newInvest = Investments.Table.NewRow();
                newInvest["clientId"] = SelectedClient["id"];
                newInvest["investmentType"] = type.ToString();
                newInvest["investmentDate"] = DateTime.Now.ToShortDateString();
                newInvest["investmentSum"] = result;
                newInvest["percentage"] = (string)SelectedClient.Row["clientType"] == "Juridical"
                                            ? 9
                                            : (string)SelectedClient.Row["clientType"] == "VIP"
                                            ? 15
                                            : 11;
                Investments.Table.Rows.Add(newInvest);
                SelectedClient.Row["bankBalance"] = (int)SelectedClient.Row["bankBalance"] - result;
                try
                {
                    Investments.Update();
                    Clients.Update();
                    MessageBox.Show("Вклад совершен!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}: ERROR");
                }
                CreateInvest.Close();
            });
            TransferButton = new Command(() =>
            {
                if (this.Transfer != null) return;
                if (SelectedClient == null) return;
                if ((int)SelectedClient["bankBalance"] < 10) { MessageBox.Show("Недостаточно денег для перевода. Минимальное количество - 10$"); return; }
                Transfer = new TransferWindow();
                Transfer.Closed += (sender, e) => { this.Transfer = null; };
                Transfer.DataContext = this;
                Transfer.ShowDialog();
            });
            TransferButtonWindow = new Command(() =>
            {
                var enumcard = Transfer.CardNumber.Text.Where(x => x != ' '); // удаляю лишние пробелы (если такие есть)
                string card = enumcard.Aggregate<char, string>(null, (current, item) => current + item);

                #region Обратные условия

                if (string.IsNullOrEmpty(card)) { MessageBox.Show("Поле пустое"); return; }
                if (!long.TryParse(card, out long result)) { MessageBox.Show("В поле для ввода карты введена строка или слишком большое число"); return; }
                if (!CheckCardNumber(result, out DataRow targetClient)) { MessageBox.Show("Такой карты не существует"); return; }
                if (!long.TryParse(Transfer.TransferSum.Text, out long sum)) { MessageBox.Show("В поле для ввода суммы введена строка или слишком большое число"); return; }
                if ((int)SelectedClient["bankBalance"] < sum) { MessageBox.Show($"Недостаточно средств для перевода. Ваши средства: {SelectedClient["bankBalance"]}$"); return; }

                #endregion
                MessageBoxResult res;
                long newsum = 0;
                switch ((string)SelectedClient["clientType"])
                {
                    case "VIP":
                        newsum = sum;
                        res = MessageBox.Show($"Комиссия перевода - 0%. Вы переведете клиенту {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient["bankBalance"] = (int)SelectedClient["bankBalance"] - newsum;
                        targetClient["bankBalance"] = (int)targetClient["bankBalance"] + newsum;
                        break;
                    case "Individual":
                        newsum = sum + (sum / 100 * 3);
                        if ((int)SelectedClient["bankBalance"] < newsum)
                        {
                            MessageBox.Show($"У вас недостаточно средств. Комиссия перевода - 3%. Вам необхрдимо {newsum}$", "Ошибка", MessageBoxButton.OK);
                            return;
                        }
                        res = MessageBox.Show($"Комиссия перевода - 3%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient["bankBalance"] = (int)SelectedClient["bankBalance"] - newsum;
                        targetClient["bankBalance"] = (int)targetClient["bankBalance"] + sum;
                        break;
                    case "Juridical":
                        newsum = sum + (sum / 100 * 2);
                        if ((int)SelectedClient["bankBalance"] < newsum)
                        {
                            MessageBox.Show($"У вас недостаточно средств. Комиссия перевода - 2%. Вам необхрдимо {newsum}$", "Ошибка", MessageBoxButton.OK);
                            return;
                        }
                        res = MessageBox.Show($"Комиссия перевода - 2%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient["bankBalance"] = (int)SelectedClient["bankBalance"] - newsum;
                        targetClient["bankBalance"] = (int)targetClient["bankBalance"] + sum;
                        break;
                    default:
                        break;
                }
                Transfer.Close();
                Clients.Update();
                DataRow paymentTransaction = Transactions.Table.NewRow();

                paymentTransaction["ClientId"] = targetClient[0];
                paymentTransaction["NameTarget"] = SelectedClient[1];
                paymentTransaction["LastnameTarget"] = SelectedClient[2];
                paymentTransaction["PatronymicTarget"] = SelectedClient[3];
                paymentTransaction["CardTarget"] = SelectedClient["cardNumber"];
                paymentTransaction["ClientTypeTarget"] = SelectedClient["clientType"];
                paymentTransaction["TransactionSum"] = sum;
                paymentTransaction["Type"] = (int)TransactionType.Receive;

                Transactions.Table.Rows.Add(paymentTransaction);

                DataRow receiveTransaction = Transactions.Table.NewRow();

                receiveTransaction["ClientId"] = SelectedClient[0];
                receiveTransaction["NameTarget"] = targetClient[1];
                receiveTransaction["LastnameTarget"] = targetClient[2];
                receiveTransaction["PatronymicTarget"] = targetClient[3];
                receiveTransaction["CardTarget"] = targetClient["cardNumber"];
                receiveTransaction["ClientTypeTarget"] = targetClient["clientType"];
                receiveTransaction["TransactionSum"] = -newsum;
                receiveTransaction["Type"] = (int)TransactionType.Payment;

                Transactions.Table.Rows.Add(receiveTransaction);

                Transactions.Update();
            });
            TransferInfo = new Command(e =>
            {
                var client = e as DataRowView;
                TransactionInfo = new TransactionInfoWindow();
                try
                {
                    TransactionInfo.DataContext = Transactions.Table.AsEnumerable().Where(x => (int)x["ClientId"] == (int)client["id"])
                        .CopyToDataTable().DefaultView;
                }
                catch
                {
                    MessageBox.Show("У выбранного клиента нет входящих или исходящих транзакций", "Ошибка", MessageBoxButton.OK);
                    return;
                }
                TransactionInfo.Show();
            });
            CopyCardNumber = new Command(e =>
            {
                var client = (DataRowView)e;
                Clipboard.SetText(client.Row["cardNumber"].ToString());
            });
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
        private DataRow RandomInvest()
        {
            DataRow data = Investments.Table.NewRow();
            data["investmentSum"] = Random.Next(500, 1000000);
            data["investmentDate"] = new DateTime(Random.Next(2015, 2020),
                Random.Next(1,13), Random.Next(1,28)).ToShortDateString();

            if (Random.Next(1,3) == 1)
            {
                data["investmentType"] = InvestmentType.Capitalization.ToString();
            }
            else
            {
                data["investmentType"] = InvestmentType.NotCapitalization.ToString();
            }
            return data;
        }
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
            if (Clients.Table.Rows.Count != 0) return;

            FillClientsInDb(Clients, count);

            Clients.Update();

            FillRandomInvestmentInClients();
        }
        private void FillRandomInvestmentInClients()
        {
            foreach (DataRow row in Clients.Table.Rows)
            {
                if (Random.Next(1, 3) == 1)
                {
                    var inv = RandomInvest();
                    if ((string)row[5] == "VIP") inv["percentage"] = 15;
                    if ((string)row[5] == "Juridical") inv["percentage"] = 9;
                    if ((string)row[5] == "Individual") inv["percentage"] = 11;
                    inv["clientId"] = row["id"];
                    Investments.Table.Rows.Add(inv);
                }
            }
            Investments.Update();
        }
        private void FillClientsInDb(ClientsDataBase db, int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataRow data = db.Table.NewRow();
                data[1] = ClientRep(0, Random);
                data[2] = ClientRep(1, Random);
                data[3] = ClientRep(2, Random);
                data[4] = Random.Next(18, 31);
                data[5] = Random.Next(1, 4) == 1 ? "Juridical" : Random.Next(1, 4) == 2
                    ? "Individual" : "VIP";
                data[6] = long.Parse(CardRandom(Random));
                data[7] = Random.Next(10, 200000);
                db.Table.Rows.Add(data);
            }
        }
        #endregion

        #region Проверки
        private bool CheckCardNumber(long cardnumber, out DataRow client)
        {
            try
            {
                client = Clients.Table.AsEnumerable().First(x => (long)x["cardNumber"] == cardnumber);
                return true;
            }
            catch
            {
                client = null;
                return false;
            }

        } // проверка на существование клиента, и возвращение его

        #endregion
    }
}

