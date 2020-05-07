using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            this.DataContext = new Bank();
        }
    }
}
#region прототип
// Создать прототип банковской системы, позвляющей управлять клиентами и клиентскими счетами.
// В информационной системе есть возможность перевода денежных средств между счетами пользователей
// Открывать вклады, с капитализацией и без
// 100 12%
// 12 ме - 112
// 100 12%
// 101 12%
// 102.01 12%

//     100
// 1   101
// 2   102,01
// 3   103,0301
// 4   104,060401
// 5   105,101005
// 6   106,1520151
// 7   107,2135352
// 8   108,2856706
// 9   109,3685273
// 10  110,4622125
// 11  111,5668347
// 12  112,682503

// * Продумать возможность выдачи кредитов
// Продумать использование обобщений

// Продемонстрировать работу созданной системы

// Банк
// ├── Отдел работы с обычными клиентами
// ├── Отдел работы с VIP клиентами
// └── Отдел работы с юридическими лицами

// Дополнительно: клиентам с хорошей кредитной историей предлагать пониженую ставку по кредиту и 
// повышенную ставку по вкладам
#endregion