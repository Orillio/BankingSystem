using MyExceptionsLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BankingSystem
{
    /// <summary>
    /// Логика взаимодействия для ClientInfoWindow.xaml
    /// </summary>
    public partial class ClientInfoWindow : Window
    {

        public event Action<int> OnWithdraw;
        public ClientInfoWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as DataRowView;
            DateTime.TryParse((string)dc.Row["investmentDate"], out var date);
            DateTime.TryParse(curDate.Text, out var currentDate);
            var days = (currentDate - date).Days;
            if (days < 0) throw new InvalidDateException($"Вклада еще несуществовало. Дата вклада: {dc.Row["investmentDate"]}"); // если дней меньше нуля, то вклада еще не существовало
            switch (dc.Row["investmentType"])
            {
                case "Capitalization": // в случае, если тип инвестиции - с капитализацией
                    var months = days / 30; // считаем количество месяцев прошедших после депозита
                    curSum.Text = ((long)((int)dc.Row["investmentSum"] + (int)dc.Row["investmentSum"] / 100.0 * (int)dc.Row["percentage"] / 12.0 * months)).ToString();
                    break;
                case "NotCapitalization":// в случае, если тип инвестиции - без капитализации
                    var years = days / 365; // считаем количество лет прошедших после депозита
                    curSum.Text = ((long)((int)dc.Row["investmentSum"] + (int)dc.Row["investmentSum"] / 100.0 * (int)dc.Row["percentage"] * years)).ToString();
                    break;
                default:
                    break;
            }

        }

        private void WithDraw_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show($"Вы уверены, что хотите вывести? Вы выведите {curSum.Text}$",
                "Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                OnWithdraw?.Invoke(int.Parse(curSum.Text));
        }
    }
}
