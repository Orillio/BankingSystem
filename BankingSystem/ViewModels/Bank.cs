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
using System.Windows.Media;
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
        public ComboBoxItem EditSelectedClientType { get; set; }
        public int SelectedDep { get; set; }
        public string SelectedDepName
        {
            get
            {
                switch (SelectedDep)
                {
                    case 0: return "Individual";
                    case 1: return "Juridical";
                    case 2: return "VIP";
                    default:
                        return null;
                }
            }
        }
        public ComboBoxItem SelectedClientType { get; set; }

        //сделай датаконтекст листвью здесь, чтобы он заполнялся от выбранного отдела
        #endregion

        #region Команды
        public ICommand InvestmentButton { get; set; } 
        public ICommand InfoClick { get; set; }
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
        public ICommand IdClick { get; set; }
        public ICommand TransferInfo { get; set; }
        public ICommand CopyCardNumber { get; set; }
        public ICommand Search { get; set; }
        public ICommand GotSearchFocus { get; set; }
        public ICommand LostSearchFocus { get; set; }

        #endregion

        #region Дополнительные поля
        bool namedesc = false;
        bool lastdesc = false;
        bool patrdesc = false;
        bool balancedesc = false;
        bool iddesc = true;

        #endregion

        #region Свойства

        public DataRowView GetInvestmentFromClient
        {
            get
            {
                var a = Investments.Table.Rows.IndexOf(Investments.Table.AsEnumerable()
                        .FirstOrDefault(x => x.Field<int>("clientId") == (int)SelectedClient[0]));
                if (a == -1) return null;
                return Investments.Table.DefaultView[a];
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

            FillClients(10);

            #region Команды

            #region Placeholder
            GotSearchFocus = new Command(e =>
            {
                var box = e as TextBox;
                if (box.Text == "Поиск")
                {
                    box.Text = string.Empty;
                    box.Foreground = Brushes.Black;
                }
            });
            LostSearchFocus = new Command(e =>
            {
                var box = e as TextBox;
                if (box.Text == string.Empty)
                {
                    box.Text = "Поиск";
                    box.Foreground = Brushes.Gray;
                }
            });

            #endregion

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
                if (targetClient == SelectedClient.Row) { MessageBox.Show("Нельзя перевести средства себе"); return; }
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
                        newsum = (long)Math.Round(sum * 1.03);
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
                        newsum = (long)Math.Round(sum * 1.02);
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
                if (result > 10000)
                {
                    MessageBox.Show("Нельзя пополнить счет на сумму более 10000$ за раз", "Ошибка", MessageBoxButton.OK);
                    return;
                }
                
                SelectedClient["bankBalance"] = (int)SelectedClient["bankBalance"] + result;
                Deposit.Close();
                Clients.Update();
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
                DataRow newClient = Clients.Table.NewRow();

                SetClientProps((string)SelectedClientType.Tag);

                AddClient.Close();
                void SetClientProps(string type)
                {
                    long card = long.Parse(CardRandom(Random));
                    while (CheckCardNumber(card, out var a))
                    {
                        card = long.Parse(CardRandom(Random));
                    }
                    newClient[1] = AddClient.Name.Text;
                    newClient[2] = AddClient.Lastname.Text;
                    newClient[3] = AddClient.Patromymic.Text;
                    newClient[4] = AddClient.Age.Text;
                    newClient[5] = type;
                    newClient[6] = card;
                    newClient[7] = 0;
                    Clients.Table.Rows.Add(newClient);
                    Clients.Update();
                }

            }); // добавление клиента
            Search = new Command(e =>
            {
                ListView clients = ((object[])e)[0] as ListView;
                string searchField = ((object[])e)[1].ToString().ToLower();

                var selectedDepartmentClients = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName);

                if (searchField == "Поиск".ToLower() || searchField == string.Empty)
                {
                    clients.ItemsSource = selectedDepartmentClients.AsDataView();
                }
                else
                {
                    clients.ItemsSource = selectedDepartmentClients.Where(x => x[0].ToString().ToLower().Contains(searchField) || x[1].ToString().ToLower().Contains(searchField)
                    || x[2].ToString().ToLower().Contains(searchField) || x[3].ToString().ToLower().Contains(searchField) || x[4].ToString().ToLower().Contains(searchField) 
                    || x[6].ToString().Equals(searchField)).AsDataView();
                }

            });
            EditClientButton = new Command(() =>
            {
                if (SelectedClient == null) { MessageBox.Show("Вы не выбрали клиента"); return; }
                EditClient = new EditClientWindow();
                if ((string)SelectedClient.Row["clientType"] == "Juridical")
                {
                    EditClient.Type.IsEnabled = false;
                }
                EditClient.DataContext = this;
                EditClient.ShowDialog();
            }); // открытие окна изменения клиента
            EditClientButtonWindow = new Command(() =>
            {
                if (EditClient.Age.Text != "" && EditClient.Age.Text.Trim(' ') != "")
                {
                    if (int.Parse(EditClient.Age.Text) > 200) { MessageBox.Show("Вы ввели нереальный возраст"); return; }
                    SelectedClient["clientAge"] = int.Parse(EditClient.Age.Text);
                }
                if (EditClient.Name.Text != "" && EditClient.Name.Text.Trim(' ') != "") SelectedClient.Row["clientName"] = EditClient.Name.Text;
                if (EditClient.Lastname.Text != "" && EditClient.Lastname.Text.Trim(' ') != "") SelectedClient.Row["clientLastname"] = EditClient.Lastname.Text;
                if (EditClient.Patronymic.Text != "" && EditClient.Patronymic.Text.Trim(' ') != "") SelectedClient.Row["clientPatronymic"] = EditClient.Patronymic.Text;
                if (EditSelectedClientType != null)
                {
                    if ((string)SelectedClient.Row["clientType"] == (string)EditSelectedClientType.Tag) { EditClient.Close(); Clients.Update(); return; }
                    else
                    {
                        SelectedClient.Row["clientType"] = (string)EditSelectedClientType.Tag; 
                        Clients.Update();
                        EditClient.Close();
                    }
                }
                else { EditClient.Close(); Clients.Update(); return; }
            }); // изменение клиента
            DeleteClient = new Command(() =>
            {
                if (SelectedClient == null) { MessageBox.Show("Клиент не выбран"); return; }
                var res = MessageBox.Show("Вы уверены, что хотите удалить клиента?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No) return;
                Clients.Table.Rows[Clients.Table.Rows.IndexOf(SelectedClient.Row)].Delete();
                Clients.Update();
            });
            NameClick = new Command(e =>
            {
                var clients = e as ListView;
                lastdesc = false;
                patrdesc = false;
                balancedesc = false;
                iddesc = true;
                if (namedesc)
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderBy(x => x["clientName"]).AsDataView();
                }
                else
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderByDescending(x => x["clientName"]).AsDataView();
                }
                namedesc = !namedesc;
            });
            LastClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                patrdesc = false;
                balancedesc = false;
                iddesc = true;
                if (lastdesc)
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderBy(x => x["clientLastname"]).AsDataView();
                else
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderByDescending(x => x["clientLastname"]).AsDataView();
                lastdesc = !lastdesc;
            });
            PatrClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                balancedesc = false;
                iddesc = true;
                if (patrdesc)
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderBy(x => x["clientPatronymic"]).AsDataView();
                }
                else
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderByDescending(x => x["clientPatronymic"]).AsDataView();
                }
                patrdesc = !patrdesc;
            });
            BalanceClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                patrdesc = false;
                iddesc = true;
                if (balancedesc)
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderBy(x => x["bankBalance"]).AsDataView();
                }
                else
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderByDescending(x => x["bankBalance"]).AsDataView();
                }
                balancedesc = !balancedesc;
            });
            IdClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                patrdesc = false;
                balancedesc = false;
                if (!iddesc)
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderBy(x => x["id"]).AsDataView();
                }
                else
                {
                    clients.ItemsSource = Clients.Table.AsEnumerable().Where(x => (string)x["clientType"] == SelectedDepName).OrderByDescending(x => x["id"]).AsDataView();
                }
                iddesc = !iddesc;
            });
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
            client = Clients.Table.AsEnumerable().FirstOrDefault(x => (long)x["cardNumber"] == cardnumber);
            if (client == default)
            {
                return false;
            }
            return true;

        } // проверка на существование клиента, и возвращение его

        #endregion
    }
}

