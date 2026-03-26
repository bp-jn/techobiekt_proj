using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace db_modelling_gui_2026.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute)
        {
            _execute = execute;
        }
        public event EventHandler CanExecuteChanged;
        
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
