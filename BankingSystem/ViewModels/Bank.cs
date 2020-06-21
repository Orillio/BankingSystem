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
using System.Net.Http.Headers;
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
        #region ObservableColls


        //Статические коллекции для ItemsSource у View
        public static ObservableCollection<Client> JuridicalClients { get; set; }
        public static ObservableCollection<Client> IndividualClients { get; set; }
        public static ObservableCollection<Client> VipClients { get; set; }
        #endregion

        #region Контекст
        public LocalDBEntities1 Context { get; set; }

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
        public Client SelectedClient { get; set; }
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

        #endregion

        #region Команды
        public ICommand InvestmentButton { get; set; }
        public ICommand InfoClick { get; set; }
        public ICommand TransferButton { get; set; }
        public ICommand TransferButtonWindow { get; set; }
        public ICommand AccountTransferButton { get; set; }
        public ICommand AccountTransferButtonWindow { get; set; }
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
        public ICommand CardBalanceClick { get; set; }
        public ICommand AccountBalanceClick { get; set; }
        public ICommand IdClick { get; set; }
        public ICommand TransferInfo { get; set; }
        public ICommand CopyCardNumberOrAccount { get; set; }
        public ICommand Search { get; set; }
        public ICommand GotSearchFocus { get; set; }
        public ICommand LostSearchFocus { get; set; }

        #endregion

        #region Дополнительные поля
        bool namedesc = false;
        bool lastdesc = false;
        bool patrdesc = false;
        bool cardbalancedesc = false;
        bool accountbalancedesc = false;
        bool iddesc = true;

        #endregion

        #region Свойства


        public Investment GetInvestmentFromClient { get => Context.Investments.FirstOrDefault(x => x.clientId == SelectedClient.Id); }

        public ObservableCollection<Client> GetCollectionFromSelectedDep
        {
            get
            {
                switch (SelectedDepName)
                {
                    case "Individual":
                        return IndividualClients;
                    case "Juridical":
                        return JuridicalClients;
                    case "VIP":
                        return VipClients;
                    default:
                        break;
                }
                return null;
            }
            set
            {
                switch (SelectedDepName)
                {
                    case "Individual":
                        IndividualClients = value;
                        break;
                    case "Juridical":
                        JuridicalClients = value;
                        break;
                    case "VIP":
                        VipClients = value;
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Конструктор

        public Bank()
        {
            Context = new LocalDBEntities1();

            FillClients(50);

            #region Контекст и коллекции сущностей для View

            JuridicalClients = new ObservableCollection<Client>(Context.Clients.Where(x => x.clientType == "Juridical"));
            IndividualClients = new ObservableCollection<Client>(Context.Clients.Where(x => x.clientType == "Individual"));
            VipClients = new ObservableCollection<Client>(Context.Clients.Where(x => x.clientType == "VIP"));

            #endregion


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
                    return;
                }
                DateTime.TryParse(GetInvestmentFromClient.investmentDate, out var date);

                var days = (CurrentDate - date).Days;
                if (days < 0) { MessageBox.Show($"Вклада еще не существовало. Дата вклада: {GetInvestmentFromClient.investmentDate}"); return; }
                Info = new ClientInfoWindow
                {
                    DataContext = GetInvestmentFromClient
                };
                Info.OnWithdraw += (x) =>
                {
                    if (SelectedClient.clientType == "Juridical")
                    {
                        SelectedClient.accountBalance += x;
                        Context.Investments.Remove(GetInvestmentFromClient);
                    }
                    else
                    {
                        SelectedClient.bankBalance += x;
                        Context.Investments.Remove(GetInvestmentFromClient);
                    }

                    Context.SaveChanges();
                    Info.Close();
                };

                Info.Closed += (sender, ex) => { this.Info = null; };
                Info.WithDraw.DataContext = this;
                Info.CurrentDate.DataContext = CurrentDate.ToShortDateString();
                Info.ShowDialog();

            });
            InvestmentButton = new Command(() =>
            {
                #region Обратные условия
                if (string.IsNullOrEmpty(CreateInvest.InvestField.Text)) { MessageBox.Show("Ничего не введено"); return; }
                if (!int.TryParse(CreateInvest.InvestField.Text, out int result)) { MessageBox.Show("Введена строка или слишком большое число"); return; }


                if (SelectedClient.clientType == "Juridical")
                {
                    if (SelectedClient.accountBalance < result)
                    {
                        MessageBox.Show($"Недостаточно средств для вклада. Ваши средства:" +
                                        $" {SelectedClient.accountBalance}$"); return;
                    }

                    if (result < 500)
                    {
                        MessageBox.Show($"Минимальное количество средств для вклада - 500$. Ваши средства:" +
                                        $" {SelectedClient.accountBalance}$"); return;
                    }
                }
                else
                {
                    if (SelectedClient.bankBalance < result)
                    {
                        MessageBox.Show($"Недостаточно средств для вклада. Ваши средства:" +
                                        $" {SelectedClient.bankBalance}$"); return;
                    }

                    if (result < 500)
                    {
                        MessageBox.Show($"Минимальное количество средств для вклада - 500$. Ваши средства:" +
                                        $" {SelectedClient.bankBalance}$"); return;
                    }
                }

                if (SelectedInvType == null) { MessageBox.Show("Вы не выбрали тип вклада"); return; }

                #endregion

                InvestmentType type = (string)SelectedInvType.Content == "С капитализацией" ? InvestmentType.Capitalization : InvestmentType.NotCapitalization;
                Investment newInvest = new Investment
                {
                    clientId = SelectedClient.Id,
                    investmentDate = DateTime.Now.ToShortDateString(),
                    investmentType = type.ToString(),
                    investmentSum = result,
                    percentage = SelectedClient.clientType == "Juridical"
                                            ? 9
                                            : (string)SelectedClient.clientType == "VIP"
                                            ? 15
                                            : 11
                };
                Context.Investments.Add(newInvest);
                if (SelectedClient.clientType == "Juridical")
                {
                    SelectedClient.accountBalance = SelectedClient.accountBalance - result;
                }
                else
                {
                    SelectedClient.bankBalance = SelectedClient.bankBalance - result;
                }

                try
                {
                    Context.SaveChanges();
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

                Transfer = new TransferWindow();

                if (SelectedClient.clientType == "Juridical")
                {
                    if (SelectedClient.accountBalance < 10) { MessageBox.Show("Недостаточно денег для перевода. Минимальное количество - 20$"); return; }
                    Transfer.InputCardnumber.Text = "Введите расчетный счет получателя";
                }
                else
                {
                    if (SelectedClient.bankBalance < 10) { MessageBox.Show("Недостаточно денег для перевода. Минимальное количество - 10$"); return; }
                }
                Transfer.Closed += (sender, e) => { this.Transfer = null; };
                Transfer.DataContext = this;
                Transfer.ShowDialog();
            });
            TransferButtonWindow = new Command(() =>
            {
                string card = Transfer.CardNumber.Text;
                if (SelectedClient.clientType != "Juridical")
                {
                    var enumcard = Transfer.CardNumber.Text.Where(x => x != ' '); // удаляю лишние пробелы (если такие есть)
                    card = enumcard.Aggregate<char, string>(null, (current, item) => current + item);
                }

                #region Обратные условия

                if (string.IsNullOrEmpty(card)) { MessageBox.Show("Поле пустое"); return; }

                Client targetClient = null;
                int sum = 0;

                if ((string)SelectedClient.clientType != "Juridical")
                {
                    if (!long.TryParse(card, out long result)) { MessageBox.Show("В поле для ввода карты введена строка или слишком большое число"); return; }
                    if (!CheckCardNumber(result, out targetClient)) { MessageBox.Show("Такой карты не существует"); return; }
                    if (!int.TryParse(Transfer.TransferSum.Text, out sum)) { MessageBox.Show("В поле для ввода суммы введена строка или слишком большое число"); return; }
                    if ((int)SelectedClient.bankBalance < sum) { MessageBox.Show($"Недостаточно средств для перевода. Ваши средства:" +
                        $" {SelectedClient.bankBalance}$"); return; }
                }
                else
                {
                    if (!CheckAccount(card, out targetClient)) { MessageBox.Show("Такого расчетного счета не существует"); return; }
                    if (!int.TryParse(Transfer.TransferSum.Text, out sum))
                    { MessageBox.Show("В поле для ввода суммы введена строка или слишком большое число"); return; }
                    if (SelectedClient.accountBalance < sum) { MessageBox.Show($"Недостаточно средств для перевода. Ваши средства:" +
                        $" {SelectedClient.accountBalance}$"); return; }
                }

                if (targetClient == SelectedClient) { MessageBox.Show("Нельзя перевести средства себе"); return; }

                #endregion

                MessageBoxResult res;
                int newsum = 0;

                switch (SelectedClient.clientType)
                {
                    case "VIP":
                        newsum = sum;
                        res = MessageBox.Show($"Комиссия перевода - 0%. Вы переведете клиенту {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient.bankBalance = SelectedClient.bankBalance - newsum;
                        targetClient.bankBalance = targetClient.bankBalance + newsum;
                        break;
                    case "Individual":
                        newsum = (int)Math.Round(sum * 1.03);
                        if (SelectedClient.bankBalance < newsum)
                        {
                            MessageBox.Show($"У вас недостаточно средств. Комиссия перевода - 3%. Вам необхрдимо {newsum}$", "Ошибка", MessageBoxButton.OK);
                            return;
                        }
                        res = MessageBox.Show($"Комиссия перевода - 3%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient.bankBalance = (int)SelectedClient.bankBalance - newsum;
                        targetClient.bankBalance = (int)targetClient.bankBalance + sum;
                        break;
                    case "Juridical":
                        newsum = (int)Math.Round(sum * 1.02);
                        if (SelectedClient.accountBalance < newsum)
                        {
                            MessageBox.Show($"У вас недостаточно средств. Комиссия перевода - 2%. Вам необхрдимо {newsum}$", "Ошибка", MessageBoxButton.OK);
                            return;
                        }
                        res = MessageBox.Show($"Комиссия перевода - 2%. У вас отнимется {newsum}$", "Информация", MessageBoxButton.YesNo);
                        if (res != MessageBoxResult.Yes) return;
                        SelectedClient.accountBalance = SelectedClient.accountBalance - newsum;
                        targetClient.accountBalance = targetClient.accountBalance + sum;
                        break;
                    default:
                        break;
                }

                Transfer.Close();
                Transaction paymentTransaction = new Transaction();

                paymentTransaction.ClientId = targetClient.Id;
                paymentTransaction.NameTarget = SelectedClient.clientName;
                paymentTransaction.LastnameTarget = SelectedClient.clientLastname;
                paymentTransaction.PatronymicTarget = SelectedClient.clientPatronymic;

                if (SelectedClient.clientType != "Juridical")
                    paymentTransaction.CardTarget = SelectedClient.cardNumber;
                else
                    paymentTransaction.CheckingAccount = SelectedClient.checkingAccount;

                paymentTransaction.ClientTypeTarget = SelectedClient.clientType;
                paymentTransaction.TransactionSum = sum;
                paymentTransaction.Type = (int)TransactionType.Receive;

                Transaction receiveTransaction = new Transaction();

                receiveTransaction.ClientId = SelectedClient.Id;
                receiveTransaction.NameTarget = targetClient.clientName;
                receiveTransaction.LastnameTarget = targetClient.clientLastname;
                receiveTransaction.PatronymicTarget = targetClient.clientPatronymic;

                if (SelectedClient.clientType != "Juridical")
                    receiveTransaction.CardTarget = targetClient.cardNumber;
                else
                    receiveTransaction.CheckingAccount = targetClient.checkingAccount;

                receiveTransaction.ClientTypeTarget = targetClient.clientType;
                receiveTransaction.TransactionSum = -newsum;
                receiveTransaction.Type = (int)TransactionType.Payment;

                Context.Transactions.Add(paymentTransaction);
                Context.Transactions.Add(receiveTransaction);

                Context.SaveChanges();
            });
            TransferInfo = new Command(e =>
            {
                TransactionInfo = new TransactionInfoWindow();
                var client = e as Client;
                if (client.clientType == "Juridical")
                {
                    Binding accountBinding = new Binding("CheckingAccount");

                    TransactionInfo.PaymentSource.DisplayMemberBinding = accountBinding;
                    (TransactionInfo.PaymentSource.Header as GridViewColumnHeader).Content = "Расчетный счет";
                    (TransactionInfo.PaymentSource.Header as GridViewColumnHeader).Width += 10;
                }
                TransactionInfo.DataContext = Context.Transactions.Where(x => x.ClientId == client.Id).ToList();
                if ((TransactionInfo.DataContext as List<Transaction>).Count == 0)
                {
                    MessageBox.Show("У выбранного клиента нет входящих или исходящих транзакций", "Ошибка", MessageBoxButton.OK);
                    return;
                }
                TransactionInfo.Show();
            });
            CopyCardNumberOrAccount = new Command(e =>
            {
                var client = (Client)e;

                if (SelectedClient.clientType == "Juridical")
                {
                    Clipboard.SetText(client.checkingAccount);
                }
                else
                {
                    Clipboard.SetText(client.cardNumber.ToString());
                }
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
                if (!int.TryParse(Deposit.DepositField.Text, out int result)) { MessageBox.Show("В поле ввода строка или число слишком большое"); return; }
                if (result > 10000)
                {
                    MessageBox.Show("Нельзя пополнить счет на сумму более 10000$ за раз", "Ошибка", MessageBoxButton.OK);
                    return;
                }
                if (SelectedClient.clientType == "Juridical")
                    SelectedClient.accountBalance += result;
                else
                    SelectedClient.bankBalance += result;

                Deposit.Close();
                Context.SaveChanges();
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
                Client newClient = new Client();

                SetClientProps((string)SelectedClientType.Tag);

                Context.Clients.Add(newClient);
                switch ((string)SelectedClientType.Tag)
                {
                    case "Juridical":
                        JuridicalClients.Add(newClient);
                        break;
                    case "Individual":
                        IndividualClients.Add(newClient);
                        break;
                    case "VIP":
                        VipClients.Add(newClient);
                        break;
                    default:
                        break;
                }
                Context.SaveChanges();

                AddClient.Close();

                void SetClientProps(string type)
                {
                    long card = long.Parse(CardRandom(Random));
                    while (CheckCardNumber(card, out var a))
                    {
                        card = long.Parse(CardRandom(Random));
                    }
                    newClient.clientName = AddClient.Name.Text;
                    newClient.clientLastname = AddClient.Lastname.Text;
                    newClient.clientPatronymic = AddClient.Patromymic.Text;
                    newClient.clientAge = res;
                    newClient.clientType = type;
                    if (type != "Juridical")
                    {
                        newClient.cardNumber = card;
                        newClient.bankBalance = 0;
                    }
                    else
                    {
                        newClient.checkingAccount = RandomCheckingAccount();
                        newClient.accountBalance = 0;
                    }
                }

            }); // добавление клиента
            Search = new Command(e =>
            {
                ListView clients = ((object[])e)[0] as ListView;
                string searchField = ((object[])e)[1].ToString().ToLower();

                var selectedDepartmentClients = Context.Clients.Where(x => x.clientType == SelectedDepName);
                if (searchField == "Поиск".ToLower() || searchField == string.Empty)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(selectedDepartmentClients);
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(selectedDepartmentClients.Where(x => x.clientName.ToLower().Contains(searchField)
                            || x.clientLastname.ToLower().Contains(searchField) || x.clientPatronymic.ToLower().Contains(searchField)
                            || x.Id.ToString().ToLower().Contains(searchField) || x.checkingAccount == searchField || x.cardNumber.ToString() == searchField));
                    clients.ItemsSource = GetCollectionFromSelectedDep;

                }

            });
            EditClientButton = new Command(() =>
            {
                if (SelectedClient == null) { MessageBox.Show("Вы не выбрали клиента"); return; }
                EditClient = new EditClientWindow();
                if (SelectedClient.clientType == "Juridical")
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
                    SelectedClient.clientAge = int.Parse(EditClient.Age.Text);
                }
                if (EditClient.Name.Text != "" && EditClient.Name.Text.Trim(' ') != "") SelectedClient.clientName = EditClient.Name.Text;
                if (EditClient.Lastname.Text != "" && EditClient.Lastname.Text.Trim(' ') != "") SelectedClient.clientLastname = EditClient.Lastname.Text;
                if (EditClient.Patronymic.Text != "" && EditClient.Patronymic.Text.Trim(' ') != "") SelectedClient.clientPatronymic = EditClient.Patronymic.Text;
                if (EditSelectedClientType != null)
                {
                    if (SelectedClient.clientType == (string)EditSelectedClientType.Tag) { EditClient.Close(); Context.SaveChanges(); return; }
                    else
                    {
                        SelectedClient.clientType = (string)EditSelectedClientType.Tag;

                        if (SelectedDepName == "Individual" && (string)EditSelectedClientType.Tag == "VIP")
                        {
                            IndividualClients.Remove(SelectedClient);
                            VipClients.Add(SelectedClient);
                        }
                        else
                        {
                            VipClients.Remove(SelectedClient);
                            IndividualClients.Add(SelectedClient);
                        }
                        Context.SaveChanges();
                        EditClient.Close();
                    }
                }
                else { EditClient.Close(); Context.SaveChanges(); return; }
            }); // изменение клиента
            DeleteClient = new Command(e =>
            {
                if (SelectedClient == null) { MessageBox.Show("Клиент не выбран"); return; }
                var res = MessageBox.Show("Вы уверены, что хотите удалить клиента?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No) return;

                Context.Clients.Remove(SelectedClient);

                switch (SelectedClient.clientType)
                {
                    case "VIP": VipClients.Remove(SelectedClient); break;
                    case "Individual": IndividualClients.Remove(SelectedClient); break;
                    case "Juridical": JuridicalClients.Remove(SelectedClient); break;
                    default:
                        break;
                }
                Context.SaveChanges();
            });

            NameClick = new Command(e =>
            {
                var clients = e as ListView;
                lastdesc = false;
                patrdesc = false;
                cardbalancedesc = false;
                iddesc = true;
                if (!namedesc)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderBy(x => x.clientName));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderByDescending(x => x.clientName));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                namedesc = !namedesc;
            });
            LastClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                patrdesc = false;
                cardbalancedesc = false;
                iddesc = true;
                if (!lastdesc)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderBy(x => x.clientLastname));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderByDescending(x => x.clientLastname));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                lastdesc = !lastdesc;
            });
            PatrClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                cardbalancedesc = false;
                iddesc = true;
                if (!patrdesc)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderBy(x => x.clientPatronymic));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderByDescending(x => x.clientPatronymic));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                patrdesc = !patrdesc;
            });
            CardBalanceClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                patrdesc = false;
                iddesc = true;
                accountbalancedesc = false;
                if (!cardbalancedesc)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderBy(x => x.bankBalance));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderByDescending(x => x.bankBalance));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                cardbalancedesc = !cardbalancedesc;
            });
            AccountBalanceClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                patrdesc = false;
                iddesc = true;
                cardbalancedesc = false;
                if (!accountbalancedesc)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderBy(x => x.accountBalance));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderByDescending(x => x.accountBalance));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                accountbalancedesc = !accountbalancedesc;
            });
            IdClick = new Command(e =>
            {
                var clients = e as ListView;
                namedesc = false;
                lastdesc = false;
                patrdesc = false;
                cardbalancedesc = false;
                if (!iddesc)
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderBy(x => x.Id));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                else
                {
                    GetCollectionFromSelectedDep = new ObservableCollection<Client>(GetCollectionFromSelectedDep.OrderByDescending(x => x.Id));
                    clients.ItemsSource = GetCollectionFromSelectedDep;
                }
                iddesc = !iddesc;
            });
            #endregion

        }
        #endregion

        #region Рандомное заполнение

        private string RandomCheckingAccount()
        {
            string basestring = "40817810099";
            var checkaccount = basestring + Random.Next(100000000, 999999999).ToString();

            //while (AccExists(checkaccount))
            //{
                checkaccount = basestring + Random.Next(100000000, 999999999).ToString();
            //}
            return checkaccount;

            //bool AccExists(string account)
            //{
            //    if (ClientsDB == null) return false;
            //    foreach (var acc in ClientsDB.Table.Rows)
            //    {
            //        if ((acc as DataRow)["checkingAccount"] != null)
            //            if ((acc as DataRow)["checkingAccount"].ToString().Equals(account)) return true;
            //    }
            //    return false;
            //}
        }

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

        private Investment RandomInvest()
        {
            Investment investment = new Investment();
            investment.investmentSum = Random.Next(500, 1000000);
            investment.investmentDate = new DateTime(Random.Next(2015, 2020),
                Random.Next(1, 13), Random.Next(1, 28)).ToShortDateString();

            if (Random.Next(1, 3) == 1)
            {
                investment.investmentType = InvestmentType.Capitalization.ToString();
            }
            else
            {
                investment.investmentType = InvestmentType.NotCapitalization.ToString();
            }
            return investment;
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
            if (Context.Clients.Count() != 0) return;

            FillClientsInDb(count);

            FillRandomInvestmentInClients();

            Context.SaveChanges();
        }

        private void FillRandomInvestmentInClients()
        {
            foreach (Client client in Context.Clients)
            {
                if (Random.Next(1, 3) == 1)
                {
                    var inv = RandomInvest();
                    if (client.clientType == "VIP") inv.percentage = 15;
                    if (client.clientType == "Juridical") inv.percentage = 9;
                    if (client.clientType == "Individual") inv.percentage = 11;
                    inv.clientId = client.Id;
                    Context.Investments.Add(inv);
                }
            }
        }

        private void FillClientsInDb(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Client client = new Client();
                client.clientName = ClientRep(0, Random);
                client.clientLastname = ClientRep(1, Random);
                client.clientPatronymic = ClientRep(2, Random);
                client.clientAge = Random.Next(18, 31);
                client.clientType = Random.Next(1, 4) == 1 ? "Juridical" : Random.Next(1, 4) == 2
                    ? "Individual" : "VIP";
                if (client.clientType == "Juridical")
                {
                    client.checkingAccount = RandomCheckingAccount();
                    client.accountBalance = Random.Next(10000, 599999);
                }
                else
                {
                    client.cardNumber = long.Parse(CardRandom(Random));
                    client.bankBalance = Random.Next(10, 200000);
                }
                Context.Clients.Add(client);
                Context.SaveChanges();
            }
        }

        #endregion

        #region Проверки
        private bool CheckCardNumber(long cardnumber, out Client client)
        {
            client = Context.Clients.FirstOrDefault(x => x.cardNumber == cardnumber);

            if (client == default)
            {
                return false;
            }
            return true;

        }
        private bool CheckAccount(string accountnumber, out Client client)
        {
            client = Context.Clients.FirstOrDefault(x => x.checkingAccount.Equals(accountnumber));

            if (client == default)
            {
                return false;
            }
            return true;

        }

        #endregion
    }
}

