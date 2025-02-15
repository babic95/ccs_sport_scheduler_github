using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir_Logging;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class OpenChangeDriverCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenChangeDriverCommand(DriverViewModel currentViewModel)
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
                    _currentViewModel.CurrentNedodeljenaPorudzbina = null;

                    _currentViewModel.CurrentPorudzbina = _currentViewModel.CurrentIsporuka.DriverInvoices.FirstOrDefault(invoice => 
                    invoice.Invoice.Id == parameter.ToString());

                    if(_currentViewModel.CurrentPorudzbina != null)
                    {
                        _currentViewModel.Window = new SelectDriverWindow(_currentViewModel);
                        _currentViewModel.Window.ShowDialog();

                        _currentViewModel.WindowIsporuka.Close();

                        _currentViewModel.CurrentDriver = null;
                        _currentViewModel.CurrentPorudzbina = null;

                        _currentViewModel.Initialize();

                        MessageBox.Show("Usprešno dodate vozač u porudzbinu!",
                                "Usprešno",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ChangeDriverCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}