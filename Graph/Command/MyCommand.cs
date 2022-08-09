using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Graph.Command
{
    internal class MyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> action;

        public MyCommand(Action<object> _action)
        {
            action = _action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
