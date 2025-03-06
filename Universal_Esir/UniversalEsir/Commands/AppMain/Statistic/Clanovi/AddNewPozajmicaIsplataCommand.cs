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
    public class AddNewPozajmicaIsplataCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public AddNewPozajmicaIsplataCommand(ClanoviViewModel currentViewModel)
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
                _currentViewModel.CurrentZaduzenje = new Zaduzenje()
                {
                    Date = DateTime.Now,
                    TotalAmountString = "0",
                    Opis = string.Empty,
                };

                _currentViewModel.CurrentWindow = new AddNewPozajmicaIsplataWindow(_currentViewModel);
                _currentViewModel.CurrentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("AddNewPozajmicaIsplataCommand -> Greska prilikom otvaranja prozora za isplatu pozajmice", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za isplatu pozajmice!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
