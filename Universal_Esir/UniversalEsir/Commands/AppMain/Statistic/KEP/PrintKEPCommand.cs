using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Printer;
using UniversalEsir_Report.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.KEP
{
    public class PrintKEPCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private KEPViewModel _currentViewModel;

        public PrintKEPCommand(KEPViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(_currentViewModel.ItemsKEP != null &&
                _currentViewModel.ItemsKEP.Any())
            {
                List<ItemKEP> kep = new List<ItemKEP>();

                _currentViewModel.ItemsKEP.ToList().ForEach(item =>
                {
                    ItemKEP itemKEP = new ItemKEP()
                    {
                        Id = item.Id,
                        Description = item.Description,
                        KepDate = item.KepDate,
                        Razduzenje = item.Razduzenje,
                        Zaduzenje = item.Zaduzenje,
                        Saldo = item.Saldo,
                    };
                    kep.Add(itemKEP);
                });

                PrinterManager.Instance.PrintKEP(_currentViewModel.FromDate, _currentViewModel.ToDate, kep);
            }
        }
    }
}