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
using UniversalEsir.Models.AppMain.Statistic.Tereni;
using UniversalEsir_Logging;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Tereni;

namespace UniversalEsir.Commands.AppMain.Tereni
{
    public class AddNewTerenCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public AddNewTerenCommand(ClanoviViewModel currentViewModel)
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
                _currentViewModel.CurrentTeren = new Teren()
                {
                    Id = -1
                };

                _currentViewModel.CurrentWindow = new AddNewTerenWindow(_currentViewModel);
                _currentViewModel.CurrentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("AddNewTerenCommand -> Greska prilikom otvaranja prozora za dodavanje terena", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za dodavanje terena!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
