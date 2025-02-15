using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class OpenEditIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenEditIsporukaCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                if(parameter != null &&
                    parameter is string)
                {
                    _currentViewModel.CurrentIsporuka = _currentViewModel.AllIsporuke.FirstOrDefault(isporuka => 
                    isporuka.Id == parameter.ToString());

                    if(_currentViewModel.CurrentIsporuka != null)
                    {
                        decimal totalAmount = _currentViewModel.CurrentIsporuka.TotalAmount;

                        _currentViewModel.IsAllSelected = true;

                        _currentViewModel.CurrentIsporuka.TotalAmount = totalAmount;

                        _currentViewModel.WindowItemsInInvoice = new EditIsporukaWindow(_currentViewModel);
                        _currentViewModel.WindowItemsInInvoice.ShowDialog();

                        _currentViewModel.WindowIsporuka.Close();

                        _currentViewModel.Initialize();
                    }
                }                               
            }
            catch (Exception ex)
            {
                Log.Error($"OpenEditIsporukaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}