using System;
using System.Windows.Input;

namespace ComandLib
{
    public class Command : ICommand
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
