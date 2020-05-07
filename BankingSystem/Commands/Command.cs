using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

namespace BankingSystem
{
    class Command : ICommand
    {
        Action mAction;
        public Command(Action action)
        {
            this.mAction = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            mAction();
        }

        public event EventHandler CanExecuteChanged;
    }
}
