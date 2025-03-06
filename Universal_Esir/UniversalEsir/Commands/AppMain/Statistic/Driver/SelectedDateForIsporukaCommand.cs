using UniversalEsir.Models.AppMain.Statistic.Driver;
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

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class SelectedDateForIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public SelectedDateForIsporukaCommand(DriverViewModel currentViewModel)
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
                if (!_currentViewModel.CurrentIsporuka.DateIsporuka.HasValue)
                {
                    Log.Error($"SelectedDateForIsporukaCommand -> Execute -> Datum isporuke je NULL!");
                    MessageBox.Show("Desila se neočekivana greška.\nObratite se serviseru.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if (_currentViewModel.CurrentIsporuka.DateIsporuka.Value.Date < DateTime.Now.Date)
                {
                    Log.Error($"SelectedDateForIsporukaCommand -> Execute -> Datum isporuke ne sme biti u proslosti!");
                    MessageBox.Show("Datum isporuke ne sme biti u prošlosti.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                _currentViewModel.Window.Close();
            }
            catch (Exception ex)
            {
                Log.Error($"SelectedDateForIsporukaCommand -> Execute -> Neocekivana greska: ", ex);
                MessageBox.Show("Desila se neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}