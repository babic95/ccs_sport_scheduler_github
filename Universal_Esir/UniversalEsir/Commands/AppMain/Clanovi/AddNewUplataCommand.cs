using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.Models.AppMain.Statistic.Tereni;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Tereni;
using UniversalEsir_Logging;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Clanovi;

namespace UniversalEsir.Commands.AppMain.Clanovi
{
    public class AddNewUplataCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public AddNewUplataCommand(ClanoviViewModel currentViewModel)
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
                    TotalAmount = 0,
                };

                _currentViewModel.CurrentWindow = new AddNewUplataWindow(_currentViewModel);
                _currentViewModel.CurrentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("AddNewUplataCommand -> Greska prilikom otvaranja prozora za dodavanje uplate", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za dodavanje uplate!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
