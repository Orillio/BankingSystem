using System.Data;
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
            Clients.ItemsSource = Deps.SelectedIndex == 0
                ? (DataContext as Bank).JuridClients.Table.DefaultView : Deps.SelectedIndex == 1
                ? (DataContext as Bank).IndivClients.Table.DefaultView : (DataContext as Bank).VIPClients.Table.DefaultView;
        }
    }
}
