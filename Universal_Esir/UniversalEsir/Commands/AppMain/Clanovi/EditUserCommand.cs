using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalEsir.State.Navigators;
using UniversalEsir.ViewModels.AppMain;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain.Statistic;

namespace UniversalEsir.Commands.AppMain.Clanovi
{
    public class EditUserCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public EditUserCommand(ClanoviViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
        }
    }
}
