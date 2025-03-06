using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
using UniversalEsir_Database;
using UniversalEsir_Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class AddDriverOnInvoiceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public AddDriverOnInvoiceCommand(DriverViewModel currentViewModel)
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
                if (parameter != null && 
                    parameter is string)
                {
                    _currentViewModel.CurrentPorudzbina = null;

                    _currentViewModel.CurrentNedodeljenaPorudzbina = _currentViewModel.AllNedodeljenePorudzbine.FirstOrDefault(invoice => 
                    invoice.Invoice.Id == parameter.ToString());

                    if (_currentViewModel.CurrentNedodeljenaPorudzbina != null)
                    {
                        _currentViewModel.Window = new SelectDriverWindow(_currentViewModel);
                        _currentViewModel.Window.ShowDialog();

                        _currentViewModel.WindowIsporuka.Close();

                        _currentViewModel.CurrentDriver = null;

                        MessageBox.Show("Usprešno dodat vozač u porudzbinu!",
                                "Usprešno",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"AddDriverOnInvoiceCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}