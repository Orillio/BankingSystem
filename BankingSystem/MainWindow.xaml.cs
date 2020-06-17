using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace BankingSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ToDo
        // 1) Сделать 3 View для ListView 
        // под разные типы клиентов с привязками (по возможности MVVM)
        // 2) Сделать новые данные под клиентов для расчетных счетов и огрнип 
        //
        GridView IndividualView;
        GridView JuridicalView;
        GridView VipView;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Bank();
            InitializeViews();
        }

        private void Deps_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DepositField.Text = "";
            if (Deps.SelectedIndex == 0)
            {
                Clients.View = IndividualView;
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "Individual").AsDataView();
            }
            else if(Deps.SelectedIndex == 1)
            {

                Clients.View = JuridicalView;
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "Juridical").AsDataView();
            }
            else
            {
                Clients.View = VipView;
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "VIP").AsDataView();
            }
        }
        private void Clients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Clients.SelectedIndex == -1) return;
            DataRow client = null;
            if ((DataContext as Bank).GetInvestmentFromClient != null)
            {
                client = (DataContext as Bank).GetInvestmentFromClient.Row;
            }
            else
            {
                DepositField.Text = "Отсутствует";
                return;
            }

            DepositField.Text = client.Field<string>(2) == "NotCapitalization"
                ? "Без капитализации"
                : "С капитализацией";
        }

        /// <summary>
        /// Создает 3 GridView для разных видов клиентов
        /// </summary>
        private void InitializeViews()
        {
            IndividualView = new GridView();
            JuridicalView = new GridView();
            VipView = new GridView();

            #region Individual поля
            {
                GridViewColumn id = new GridViewColumn();
                id.Width = 60;
                var idheader = new GridViewColumnHeader();
                idheader.Command = (DataContext as Bank).IdClick;
                idheader.CommandParameter = Clients;
                idheader.Content = "id";
                id.Header = idheader;
                id.DisplayMemberBinding = new Binding("id");

                GridViewColumn name = new GridViewColumn();
                name.Width = 110;
                var nameheader = new GridViewColumnHeader();
                nameheader.Command = (DataContext as Bank).NameClick;
                nameheader.CommandParameter = Clients;
                nameheader.Content = "Имя";
                name.Header = nameheader;
                name.DisplayMemberBinding = new Binding("clientName");

                GridViewColumn lastname = new GridViewColumn();
                lastname.Width = 110;
                var lastnameheader = new GridViewColumnHeader();
                lastnameheader.Command = (DataContext as Bank).LastClick;
                lastnameheader.CommandParameter = Clients;
                lastnameheader.Content = "Фамилия";
                lastname.Width = 110;
                lastname.Header = lastnameheader;
                lastname.DisplayMemberBinding = new Binding("clientLastname");

                GridViewColumn patronymic = new GridViewColumn();
                var patronymicheader = new GridViewColumnHeader();
                patronymicheader.Command = (DataContext as Bank).PatrClick;
                patronymicheader.CommandParameter = Clients;
                patronymicheader.Content = "Отчество";
                patronymic.Width = 110;
                patronymic.Header = patronymicheader;
                patronymic.DisplayMemberBinding = new Binding("clientPatronymic");

                GridViewColumn card = new GridViewColumn();
                var cardheader = new GridViewColumnHeader();
                cardheader.Content = "Номер карты";
                card.Width = 140;
                card.Header = cardheader;
                card.DisplayMemberBinding = new Binding("cardNumber");

                GridViewColumn balance = new GridViewColumn();
                var balanceheader = new GridViewColumnHeader();
                balanceheader.Content = "Баланс карты";
                balance.Width = 110;
                balance.Header = balanceheader;
                balance.DisplayMemberBinding = new Binding("bankBalance");

                IndividualView.Columns.Add(id);
                IndividualView.Columns.Add(name);
                IndividualView.Columns.Add(lastname);
                IndividualView.Columns.Add(patronymic);
                IndividualView.Columns.Add(card);
                IndividualView.Columns.Add(balance);
            }


            #endregion

            #region Juridical поля
            {
                GridViewColumn id = new GridViewColumn();
                id.Width = 60;
                var idheader = new GridViewColumnHeader();
                idheader.Command = (DataContext as Bank).IdClick;
                idheader.CommandParameter = Clients;
                idheader.Content = "id";
                id.Header = idheader;
                id.DisplayMemberBinding = new Binding("id");

                GridViewColumn name = new GridViewColumn();
                name.Width = 110;
                var nameheader = new GridViewColumnHeader();
                nameheader.Command = (DataContext as Bank).NameClick;
                nameheader.CommandParameter = Clients;
                nameheader.Content = "Имя";
                name.Header = nameheader;
                name.DisplayMemberBinding = new Binding("clientName");

                GridViewColumn lastname = new GridViewColumn();
                lastname.Width = 110;
                var lastnameheader = new GridViewColumnHeader();
                lastnameheader.Command = (DataContext as Bank).LastClick;
                lastnameheader.CommandParameter = Clients;
                lastnameheader.Content = "Фамилия";
                lastname.Header = lastnameheader;
                lastname.DisplayMemberBinding = new Binding("clientLastname");

                GridViewColumn patronymic = new GridViewColumn();
                var patronymicheader = new GridViewColumnHeader();
                patronymicheader.Command = (DataContext as Bank).PatrClick;
                patronymicheader.CommandParameter = Clients;
                patronymicheader.Content = "Отчество";
                patronymic.Width = 110;
                patronymic.Header = patronymicheader;
                patronymic.DisplayMemberBinding = new Binding("clientPatronymic");

                JuridicalView.Columns.Add(id);
                JuridicalView.Columns.Add(name);
                JuridicalView.Columns.Add(lastname);
                JuridicalView.Columns.Add(patronymic);
            }

            #endregion

            #region VIP поля
            {
                GridViewColumn id = new GridViewColumn();
                id.Width = 60;
                var idheader = new GridViewColumnHeader();
                idheader.Command = (DataContext as Bank).IdClick;
                idheader.CommandParameter = Clients;
                idheader.Content = "id";
                id.Header = idheader;
                id.DisplayMemberBinding = new Binding("id");

                GridViewColumn name = new GridViewColumn();
                name.Width = 110;
                var nameheader = new GridViewColumnHeader();
                nameheader.Command = (DataContext as Bank).NameClick;
                nameheader.CommandParameter = Clients;
                nameheader.Content = "Имя";
                name.Header = nameheader;
                name.DisplayMemberBinding = new Binding("clientName");

                GridViewColumn lastname = new GridViewColumn();
                lastname.Width = 110;
                var lastnameheader = new GridViewColumnHeader();
                lastnameheader.Command = (DataContext as Bank).LastClick;
                lastnameheader.CommandParameter = Clients;
                lastnameheader.Content = "Фамилия";
                lastname.Width = 110;
                lastname.Header = lastnameheader;
                lastname.DisplayMemberBinding = new Binding("clientLastname");

                GridViewColumn patronymic = new GridViewColumn();
                var patronymicheader = new GridViewColumnHeader();
                patronymicheader.Command = (DataContext as Bank).PatrClick;
                patronymicheader.CommandParameter = Clients;
                patronymicheader.Content = "Отчество";
                patronymic.Width = 110;
                patronymic.Header = patronymicheader;
                patronymic.DisplayMemberBinding = new Binding("clientPatronymic");

                GridViewColumn card = new GridViewColumn();
                var cardheader = new GridViewColumnHeader();
                cardheader.Content = "Номер карты";
                card.Width = 140;
                card.Header = cardheader;
                card.DisplayMemberBinding = new Binding("cardNumber");

                GridViewColumn balance = new GridViewColumn();
                var balanceheader = new GridViewColumnHeader();
                balanceheader.Content = "Баланс карты";
                balance.Width = 140;
                balance.Header = balanceheader;
                balance.DisplayMemberBinding = new Binding("bankBalance");

                VipView.Columns.Add(id);
                VipView.Columns.Add(name);
                VipView.Columns.Add(lastname);
                VipView.Columns.Add(patronymic);
                VipView.Columns.Add(card);
                VipView.Columns.Add(balance);
            }
            #endregion
        }
    }
}
