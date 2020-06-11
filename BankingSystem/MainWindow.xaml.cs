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

        private void Deps_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
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
            DataRow client = null;
            try
            {
                 client = (DataContext as Bank).Investments.Table.AsEnumerable()
                    .First(x => x.Field<int>(1) == (Clients.SelectedItem as DataRowView).Row
                    .Field<int>(0));
            }
            catch
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
