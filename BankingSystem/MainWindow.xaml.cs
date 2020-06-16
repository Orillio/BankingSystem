using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace BankingSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Bank();
        }
        // ToDo
        // 1) Сделать 3 View для ListView 
        // под разные типы клиентов с привязками (по возможности MVVM)
        // 2) Сделать новые данные под клиентов для расчетных счетов и огрнип 
        //



        private void Deps_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DepositField.Text = "";
            if (Deps.SelectedIndex == 0)
            {
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "Individual").AsDataView();
            }
            else if(Deps.SelectedIndex == 1)
            {
                Clients.ItemsSource = (DataContext as Bank).Clients.Table.AsEnumerable()
                    .Where(x => x.Field<string>(5) == "Juridical").AsDataView();
            }
            else
            {
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
    }
}
