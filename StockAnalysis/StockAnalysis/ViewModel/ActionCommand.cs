using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalysis.ViewModel
{
    static class CommandHelper
    {
        public static ICommand Create(Action action)
        {
            return new ActionCommand(action);
        }
        public static ICommand Create(Action<object> action)
        {
            return new ActionCommandParam(action);
        }
    }

    public class ActionCommand : ICommand
    {
        public ActionCommand(Action ac)
        {
            action = ac;
        }

        public Action action { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            action.Invoke();
        }
    }

    public class ActionCommandParam : ICommand
    {
        public ActionCommandParam(Action<object> ac)
        {
            action = ac;
        }

        public Action<object> action { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            action.Invoke(parameter);
        }
    }
}
