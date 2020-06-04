using System.Data;
using System.Windows;
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

            var res = from x in (DataContext as Bank).Clients.Table.AsEnumerable()
                      where x.Field<string>("clienttype") == "Juridical"
                      select x;

            DataTable temp = res.CopyToDataTable();

            Clients.ItemsSource = temp.DefaultView;
        }
    }
}
