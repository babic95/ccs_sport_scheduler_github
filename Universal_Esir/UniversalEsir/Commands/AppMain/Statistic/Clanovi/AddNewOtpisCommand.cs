using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Clanovi;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Statistic.Clanovi
{
    public class AddNewOtpisCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public AddNewOtpisCommand(ClanoviViewModel currentViewModel)
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
                _currentViewModel.CurrentClan = _currentViewModel.Clanovi.FirstOrDefault();
                _currentViewModel.CurrentUplata = new Uplata()
                {
                    Date = DateTime.Now,
                    TotalAmountString = "0",
                    Description = string.Empty
                };

                _currentViewModel.CurrentWindow = new AddNewOtpisWindow(_currentViewModel);
                _currentViewModel.CurrentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("AddNewOtpisCommand -> Greska prilikom otvaranja prozora za dodavanje otpisa", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za dodavanje otpisa!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
