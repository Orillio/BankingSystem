using BankingSystem.DataBase;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;


namespace BankingSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GridView IndividualAndVipView;
        GridView JuridicalView;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Bank();
            InitializeViews();
        }

        private void Deps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DepositField.Text = "";
            if (Deps.SelectedIndex == 0)
            {
                CardNumberFields.Visibility = Visibility.Visible;
                CheckAccountFields.Visibility = Visibility.Collapsed;
                Clients.View = IndividualAndVipView;
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "Individual").AsDataView();
            }
            else if (Deps.SelectedIndex == 1)
            {
                CardNumberFields.Visibility = Visibility.Collapsed;
                CheckAccountFields.Visibility = Visibility.Visible;
                Clients.View = JuridicalView;
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "Juridical").AsDataView();
            }
            else
            {
                CardNumberFields.Visibility = Visibility.Visible;
                CheckAccountFields.Visibility = Visibility.Collapsed;
                Clients.View = IndividualAndVipView;
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "VIP").AsDataView();
            }
        }

        private void Clients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Clients.SelectedIndex == -1) return;
            DataRow investment = null;
            if ((DataContext as Bank).GetInvestmentFromClient != null)
            {
                investment = (DataContext as Bank).GetInvestmentFromClient.Row;
            }
            else
            {
                DepositField.Text = "Отсутствует";
                return;
            }

            DepositField.Text = investment.Field<string>(2) == "NotCapitalization"
                ? "Без капитализации"
                : "С капитализацией";
        }

        /// <summary>
        /// Создает 2 GridView для разных видов клиентов
        /// </summary>
        private void InitializeViews()
        {
            IndividualAndVipView = new GridView();
            JuridicalView = new GridView();

            #region Individual and VIP поля
            {

                GridViewColumn id = new GridViewColumn();
                id.Width = 60;
                var idheader = new GridViewColumnHeader();
                idheader.Command = (DataContext as Bank).IdClick;
                idheader.CommandParameter = Clients;
                idheader.Content = "id";
                id.Header = idheader;
                id.DisplayMemberBinding = new Binding("id");

                GridViewColumn namecolumn = (GridViewColumn)Application.Current.Resources["TransactionTemplate"];
                (namecolumn.Header as GridViewColumnHeader).Command = (DataContext as Bank).NameClick;
                (namecolumn.Header as GridViewColumnHeader).CommandParameter = Clients;

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
                var cardbinding = new Binding("cardNumber");
                cardbinding.Converter = new CardConverter();
                card.DisplayMemberBinding = cardbinding;

                GridViewColumn balance = new GridViewColumn();
                var balanceheader = new GridViewColumnHeader();
                balanceheader.Content = "Баланс карты";
                balance.Width = 110;
                balance.Header = balanceheader;
                balance.DisplayMemberBinding = new Binding("bankBalance");

                IndividualAndVipView.Columns.Add(id);
                IndividualAndVipView.Columns.Add(namecolumn);
                IndividualAndVipView.Columns.Add(lastname);
                IndividualAndVipView.Columns.Add(patronymic);
                IndividualAndVipView.Columns.Add(card);
                IndividualAndVipView.Columns.Add(balance);
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

                GridViewColumn namecolumn = (GridViewColumn)Application.Current.Resources["TransactionTemplateJurid"];
                (namecolumn.Header as GridViewColumnHeader).Command = (DataContext as Bank).NameClick;
                (namecolumn.Header as GridViewColumnHeader).CommandParameter = Clients;

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

                GridViewColumn account = new GridViewColumn();
                var accountheader = new GridViewColumnHeader();
                accountheader.Content = "Расчетный счет";
                account.Width = 150;
                account.Header = accountheader;
                account.DisplayMemberBinding = new Binding("checkingAccount");

                JuridicalView.Columns.Add(id);
                JuridicalView.Columns.Add(namecolumn);
                JuridicalView.Columns.Add(lastname);
                JuridicalView.Columns.Add(patronymic);
                JuridicalView.Columns.Add(account);
            }

            #endregion

        }
    }
}
