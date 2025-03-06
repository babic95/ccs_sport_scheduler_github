using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.ViewModels;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Nivelacija
{
    public class SearchNivelacijaItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NivelacijaViewModel _currentViewModel;

        public SearchNivelacijaItemsCommand(NivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.CurrentGroup != null)
            {
                if (_currentViewModel.CurrentGroup.Id == -1)
                {
                    _currentViewModel.SearchItems = new List<Invertory>(_currentViewModel.AllItems);
                }
                else
                {
                    _currentViewModel.SearchItems = new List<Invertory>(_currentViewModel.AllItems.Where(item =>
                    item.IdGroupItems == _currentViewModel.CurrentGroup.Id));
                }

                _currentViewModel.SearchText = string.Empty;
            }
        }
    }
}