using BankingSystem.DataBase;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

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

            var contextClients = (DataContext as Bank).Context.Clients;
            var context = (DataContext as Bank).Context;

            contextClients.Load();

            var checkBalanceBind = new Binding("SelectedItem.accountBalance");
            var cardBalanceBind = new Binding("SelectedItem.bankBalance");
            cardBalanceBind.ElementName = "Clients";
            checkBalanceBind.ElementName = "Clients";

            if (Deps.SelectedIndex == 0)
            {
                CardNumberFields.Visibility = Visibility.Visible;
                CheckAccountFields.Visibility = Visibility.Collapsed;

                AccountTransfer.Visibility = Visibility.Collapsed;
                CardTransfer.Visibility = Visibility.Visible;
                InfoBalance.SetBinding(TextBlock.TextProperty, cardBalanceBind);
                Balance.SetBinding(TextBlock.TextProperty, cardBalanceBind);

                Clients.View = IndividualAndVipView;

                Bank.IndividualClients = new ObservableCollection<Client>((DataContext as Bank).Context.Clients.Where(x => x.clientType == "Individual"));
                Clients.ItemsSource = Bank.IndividualClients;
            }
            else if (Deps.SelectedIndex == 1)
            {
                CardNumberFields.Visibility = Visibility.Collapsed;
                CheckAccountFields.Visibility = Visibility.Visible;

                AccountTransfer.Visibility = Visibility.Visible;
                CardTransfer.Visibility = Visibility.Collapsed;
                InfoBalance.SetBinding(TextBlock.TextProperty, checkBalanceBind);
                Balance.SetBinding(TextBlock.TextProperty, checkBalanceBind);

                Clients.View = JuridicalView;

                Bank.IndividualClients = new ObservableCollection<Client>((DataContext as Bank).Context.Clients.Where(x => x.clientType == "Juridical"));
                Clients.ItemsSource = Bank.JuridicalClients;
            }
            else
            {
                CardNumberFields.Visibility = Visibility.Visible;
                CheckAccountFields.Visibility = Visibility.Collapsed;

                AccountTransfer.Visibility = Visibility.Collapsed;
                CardTransfer.Visibility = Visibility.Visible;
                InfoBalance.SetBinding(TextBlock.TextProperty, cardBalanceBind);
                Balance.SetBinding(TextBlock.TextProperty, cardBalanceBind);

                Clients.View = IndividualAndVipView;
                Bank.VipClients = new ObservableCollection<Client>((DataContext as Bank).Context.Clients.Where(x => x.clientType == "VIP"));
                Clients.ItemsSource = Bank.VipClients;
            }
        }

        private void Clients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Clients.SelectedIndex == -1) return;
            Investment investment = null;
            if ((DataContext as Bank).GetInvestmentFromClient != null)
            {
                investment = (DataContext as Bank).GetInvestmentFromClient;
            }
            else
            {
                DepositField.Text = "Отсутствует";
                return;
            }

            DepositField.Text = investment.investmentType == "NotCapitalization"
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
                idheader.Content = "ID";
                id.Header = idheader;
                id.DisplayMemberBinding = new Binding("Id");

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
                balanceheader.Command = (DataContext as Bank).CardBalanceClick;
                balanceheader.CommandParameter = Clients;
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
                idheader.Content = "ID";
                id.Header = idheader;
                id.DisplayMemberBinding = new Binding("Id");

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

                GridViewColumn balance = new GridViewColumn();
                var balanceheader = new GridViewColumnHeader();
                balanceheader.Content = "Баланс счета";
                balanceheader.Command = (DataContext as Bank).AccountBalanceClick;
                balanceheader.CommandParameter = Clients;
                balance.Width = 110;
                balance.Header = balanceheader;
                balance.DisplayMemberBinding = new Binding("accountBalance");

                JuridicalView.Columns.Add(id);
                JuridicalView.Columns.Add(namecolumn);
                JuridicalView.Columns.Add(lastname);
                JuridicalView.Columns.Add(patronymic);
                JuridicalView.Columns.Add(account);
                JuridicalView.Columns.Add(balance);
            }

            #endregion

        }
    }
    
}
