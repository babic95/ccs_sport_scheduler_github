using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class EditDriverCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public EditDriverCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                var driver = _currentViewModel.AllDrivers.Where(driver => driver.Id == Convert.ToInt32(parameter)).FirstOrDefault();

                if (driver != null)
                {
                    _currentViewModel.CurrentDriver = driver;

                    _currentViewModel.Window = new AddEditDriverWindow(_currentViewModel);
                    _currentViewModel.Window.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ne postoji vozač!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"EditDriverCommand -> Execute -> Greska prilikom ucitavanja vozaca sa ID = {parameter}", ex);
                MessageBox.Show("Greška prilikom učitavanja vozača!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _currentViewModel.CurrentDriver = null;
        }
    }
}