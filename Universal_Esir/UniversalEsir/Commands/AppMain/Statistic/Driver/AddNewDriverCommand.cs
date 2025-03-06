using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class AddNewDriverCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public AddNewDriverCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.Window = new AddEditDriverWindow(_currentViewModel);
            _currentViewModel.Window.ShowDialog();

            _currentViewModel.CurrentDriver = null;
        }
    }
}