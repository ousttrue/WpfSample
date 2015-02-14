using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvalonDockUtil
{
    public class RelayCommand : ICommand
    {
        Action m_callback;

        public RelayCommand(Action callback)
        {
            m_callback = callback;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            m_callback();
        }
    }
}
