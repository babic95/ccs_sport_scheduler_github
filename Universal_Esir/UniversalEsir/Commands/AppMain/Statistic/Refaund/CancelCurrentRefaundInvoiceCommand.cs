using UniversalEsir.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.Enums.Sale;
using UniversalEsir.Models.Sale;
using UniversalEsir_Database;
using System.Collections.ObjectModel;

namespace UniversalEsir.Commands.AppMain.Statistic.Refaund
{
    public class CancelCurrentRefaundInvoiceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RefaundViewModel _currentViewModel;

        public CancelCurrentRefaundInvoiceCommand(RefaundViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da poništite refundaciju?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _currentViewModel.Initialize();
            }
        }
    }
}