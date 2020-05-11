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
        Action<object> oneParamAction;
        public Command(Action action)
        {
            this.mAction = action;
        }
        public Command(Action<object> action)
        {
            this.oneParamAction = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            mAction?.Invoke();
            oneParamAction?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
