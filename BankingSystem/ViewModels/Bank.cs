using BankingSystem.Departments;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankingSystem
{
    class Bank : PropertiesChanged // Модель представления банка для основногo окна
    {
        #region Рандом
        public Random Random { get; set; } = new Random();

        #endregion

        #region Коллекция отделов
        public ObservableCollection<BaseDepartment> DepItems { get; set; } // Коллекция с 3 отделами 
        #endregion

        #region Окна
        private ClientInfoWindow Info { get; set; } // окно с информацией 
        private InvestCreateWindow CreateInvest { get; set; }
        private TransferWindow Transfer { get; set; }
        private DepositWindow Deposit { get; set; }

        #endregion

        #region Переданные данные через View
        public AbstractClient SelectedClient { get; set; } // Информация о выбранном клиенте, которая передается через View
        public BaseDepartment SelectedDepartment { get; set; } // Информация о выбранном отделе, которая передается через View
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public ComboBoxItem SelectedInvType { get; set; }
        #endregion

        #region Команды
        public ICommand InvestmentButton { get; set; } 
        public ICommand InfoClick { get; set; } // команда для открытия окна информации о выбранном клиенте
        public ICommand WithdrawButton { get; set; }
        public ICommand TransferButton { get; set; }
        public ICommand TransferButtonWindow { get; set; }
        public ICommand DepositButton { get; set; }
        public ICommand DepositButtonWindow { get; set; }

        #endregion

        public Bank()
        {
            #region Команды

            InfoClick = new Command(ClickInfo);
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
                SelectedClient.Investment = new Investment(type, clientType, result, DateTime.Now);
                SelectedClient.BankBalance = SelectedClient.BankBalance - result;
                CreateInvest.Close();
                MessageBox.Show("Вклад оформлен!");
            });
            TransferButton = new Command(() =>
            {
                if (this.Transfer != null) return;
                if (SelectedClient == null) return;
                if (SelectedClient.BankBalance <= 10) { MessageBox.Show("Недостаточно денег для перевода. Минимальное количество - 10$"); return; }
                Transfer = new TransferWindow();
                Transfer.Closed += (sender, e) => { this.Transfer = null; };
                Transfer.DataContext = this;
                Transfer.ShowDialog();
            });
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
                        res = MessageBox.Show($"Комиссия перевода - 0%. Вы переведете клиенту {sum}$","Информация", MessageBoxButton.YesNo);
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
                MessageBox.Show("Перевод завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            });
            WithdrawButton = new Command(() =>
            {
                var res = MessageBox.Show($"Вы уверены что хотите вывести вклад? Вы выведите {SelectedClient.Investment.CurrentSum}$", "",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                SelectedClient.BankBalance += SelectedClient.Investment.CurrentSum;
                SelectedClient.Investment = null;
                Info.Close();
                MessageBox.Show("Вы вывели вклад на свой счет.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            DepositButton = new Command(() =>
            {
                Deposit = new DepositWindow();
                Deposit.DataContext = this;
                Deposit.Closed += (sender, e) => { this.Deposit = null; };
                Deposit.ShowDialog();
            });
            DepositButtonWindow = new Command(() =>
            {
                if (string.IsNullOrEmpty(Deposit.DepositField.Text)) { MessageBox.Show("Поле пустое"); return; }
                if (!long.TryParse(Deposit.DepositField.Text, out long result)) { MessageBox.Show("В поле ввода строка или число слишком большое"); return; }
                if (result > 10000) { MessageBox.Show("Нельзя пополнить счет на сумму более 10000$ за раз"); return; }
                SelectedClient.BankBalance += result;
                Deposit.Close();
                MessageBox.Show("Баланс пополнен!");
            });

            #endregion

            #region Инициализация отделов
            DepItems = new ObservableCollection<BaseDepartment>(); // добавление отделов в коллекцию
            DepItems.Add(new Department<Juridical>("Юридические лица"));
            DepItems.Add(new Department<Individual>("Физические лица"));
            DepItems.Add(new Department<VIPClient>("VIP Клиенты"));

            #endregion

            FillClients(10); // заполнение рандомными сотрудниками во всех отделах
        }

        #region Рандомное заполнение

        private string ClientRep(int type)
        {
            if (type == 0)
                switch (Random.Next(0, 31))
                {
                    case 0: return "Филипп";
                    case 1: return "Харитон";
                    case 2: return "Корнелий ";
                    case 3: return "Валерий ";
                    case 4: return "Евгений ";
                    case 5: return "Чарльз ";
                    case 6: return "Оливер ";
                    case 7: return "Цицерон ";
                    case 8: return "Ананий ";
                    case 9: return "Болеслав ";
                    case 10: return "Пётр ";
                    case 11: return "Яков ";
                    case 12: return "Борис ";
                    case 13: return "Зураб ";
                    case 14: return "Яромир ";
                    case 15: return "Закир ";
                    case 16: return "Сава ";
                    case 17: return "Никодим ";
                    case 18: return "Эдуард ";
                    case 19: return "Константин ";
                    case 20: return "Трофим ";
                    case 21: return "Орландо ";
                    case 22: return "Бронислав ";
                    case 23: return "Йозеф ";
                    case 24: return "Вячеслав ";
                    case 25: return "Борис ";
                    case 26: return "Тимофей ";
                    case 27: return "Богдан ";
                    case 28: return "Филипп";
                    case 29: return "Феликс ";
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
                            new DateTime(Random.Next(2000, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                        else return new Investment(InvestmentType.NotCapitalization, ClientType.VIP, Random.Next(500, 10000),
                            new DateTime(Random.Next(2000,DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                    else return null;
                case ClientType.Individual:
                    if (Random.Next(0, 2) == 0)
                        if (Random.Next(0, 2) == 0) return new Investment(InvestmentType.Capitalization, ClientType.Individual, Random.Next(500, 10000),
                            new DateTime(Random.Next(2000, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                        else return new Investment(InvestmentType.NotCapitalization, ClientType.Individual, Random.Next(500, 10000),
                            new DateTime(Random.Next(2000, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                    else return null;
                case ClientType.Juridical:
                    if (Random.Next(0, 2) == 0)
                        if (Random.Next(0, 2) == 0) return new Investment(InvestmentType.Capitalization, ClientType.Juridical, Random.Next(500, 10000),
                            new DateTime(Random.Next(2000, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                        else return new Investment(InvestmentType.NotCapitalization, ClientType.Juridical, Random.Next(500, 10000),
                            new DateTime(Random.Next(2000, DateTime.Today.Year - 1), Random.Next(1, 13), Random.Next(1, 28)));
                    else return null;
                default:
                    break;
            }
            return null;
        } // рандомная информация о вкладе
        private string CardRandom()
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
                (DepItems[0] as Department<Juridical>).Clients.Add(new Juridical(ClientRep(0), ClientRep(1), ClientRep(2), ClientType.Juridical, Random.Next(18,40),
                    long.Parse(CardRandom()), Random.Next(10, 100000), RandomInvest(ClientType.Juridical)));
            }
            for (int i = 0; i < count; i++)
            {
               (DepItems[1] as Department<Individual>).Clients.Add(new Individual(ClientRep(0), ClientRep(1), ClientRep(2), ClientType.Individual, Random.Next(18, 40),
                    long.Parse(CardRandom()), Random.Next(10, 100000), RandomInvest(ClientType.Individual)));
            }
            for (int i = 0; i < count; i++)
            {
                (DepItems[2] as Department<VIPClient>).Clients.Add(new VIPClient(ClientRep(0), ClientRep(1), ClientRep(2), ClientType.VIP, Random.Next(18, 40),
                    long.Parse(CardRandom()), Random.Next(10, 100000), RandomInvest(ClientType.VIP)));
            }
        } // заполнение сотрудниками
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
