using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BetterAW
{
    public class RelayCommand : ICommand
    {
        public delegate void RelayDelegate();

        private RelayDelegate _executer = null;
        public RelayDelegate Executer { set { _executer = value; } }
        public RelayCommand() { }
        public RelayCommand(RelayDelegate executer) { _executer = executer; }
        #region Fields
        event EventHandler ICommand.CanExecuteChanged
        {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object foo)
        {
            if(_executer != null)
                return true;
            return false;
        }

        public void Execute(object foo )
        {
            _executer();
        }

        #endregion // ICommand Members
    }
}
