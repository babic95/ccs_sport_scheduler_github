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
    public class AddNewClanarinaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public AddNewClanarinaCommand(ClanoviViewModel currentViewModel)
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
                _currentViewModel.SatiTermina = new System.Collections.ObjectModel.ObservableCollection<SatiTermina>();

                for (int sat = 7; sat < 23; sat++)
                {
                    _currentViewModel.SatiTermina.Add(new SatiTermina(sat));
                }

                _currentViewModel.CurrentSatiTermina = _currentViewModel.SatiTermina.FirstOrDefault();

                _currentViewModel.CurrentClan = _currentViewModel.Clanovi.FirstOrDefault();
                _currentViewModel.CurrentZaduzenje = new Zaduzenje()
                {
                    Date = DateTime.Now,
                    TotalAmountString = "0",
                    Opis = string.Empty,
                };

                _currentViewModel.CurrentWindow = new AddNewClanarinaWindow(_currentViewModel);
                _currentViewModel.CurrentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("AddNewClanarinaCommand -> Greska prilikom otvaranja prozora za dodavanje clanarine", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za dodavanje članarine!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
