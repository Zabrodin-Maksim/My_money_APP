using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace My_money.ViewModel
{
    public class MyICommand<T> : ICommand
    {
        Func<T, Task> _TargetExecuteMethod;
        Func<T, bool> _TargetCanExecuteMethod;

        public MyICommand(Func<T, Task> executeMethod)
        {
            _TargetExecuteMethod = executeMethod;
        }

        public MyICommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _TargetExecuteMethod = executeMethod;
            _TargetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {

            if (_TargetCanExecuteMethod != null)
            {
                T tparm = (T)parameter;
                return _TargetCanExecuteMethod(tparm);
            }

            if (_TargetExecuteMethod != null)
            {
                return true;
            }

            return false;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public async void Execute(object parameter)
        {
            if (_TargetExecuteMethod != null)
            {
                await _TargetExecuteMethod((T)parameter);
            }
        }

        #endregion
    }
}
