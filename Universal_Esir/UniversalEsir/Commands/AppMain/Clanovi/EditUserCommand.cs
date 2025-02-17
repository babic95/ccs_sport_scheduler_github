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
using System.Windows;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Clanovi;
using System.Windows.Controls;
using UniversalEsir_Logging;

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
            try
            {
                DataGridCellInfo dataGridCellInfo = (DataGridCellInfo)parameter;
                _currentViewModel.CurrentClan = (Clan)dataGridCellInfo.Item;

                if(_currentViewModel.CurrentClan == null)
                {
                    MessageBox.Show("Niste izabrali clana za izmenu!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                _currentViewModel.CurrentWindow = new AddEditClanWindow(_currentViewModel);
                _currentViewModel.CurrentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Error("EditUserCommand -> Greska prilikom otvaranja prozora za izmenu clana", ex);
                MessageBox.Show("Greška prilikom otvaranja prozora za izmenu clana!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
