using System;
using System.Windows.Input;

namespace CoppyWeeklyMonthly
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;


        public RelayCommand(Action<object> executionCode)
        {
            execute = executionCode;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
