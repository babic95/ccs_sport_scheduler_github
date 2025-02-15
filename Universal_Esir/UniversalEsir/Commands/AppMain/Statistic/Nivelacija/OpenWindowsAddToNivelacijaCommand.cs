using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Nivelacija;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Nivelacija
{
    public class OpenWindowsAddToNivelacijaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NivelacijaViewModel _currentViewModel;

        public OpenWindowsAddToNivelacijaCommand(NivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter is string)
            {
                string itemId = (string) parameter;

                if(!string.IsNullOrEmpty(itemId))
                {
                    var item = _currentViewModel.AllItems.FirstOrDefault(i => i.Item.Id == itemId);

                    if(item != null)
                    {
                        _currentViewModel.NewPrice = 0;
                        _currentViewModel.RazlikaCene = 0;
                        _currentViewModel.ProcenatNaDosadasnjuCenu = 0;
                        _currentViewModel.MarzaLastCalculation = 0;

                        _currentViewModel.CurrentItemsToNivelacija = new NivelacijaItem(item.Item);

                        _currentViewModel.NivelacijaItemWindow = new AddEditNivelacijaItemWindow(_currentViewModel);
                        _currentViewModel.NivelacijaItemWindow.ShowDialog();
                    }
                }
            }
        }
    }
}