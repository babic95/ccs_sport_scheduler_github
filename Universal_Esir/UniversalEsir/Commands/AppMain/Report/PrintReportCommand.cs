using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain;
using UniversalEsir_Printer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UniversalEsir_Printer.PaperFormat;

namespace UniversalEsir.Commands.AppMain.Report
{
    public class PrintReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public PrintReportCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is ReportViewModel)
            {
                ReportViewModel reportViewModel = (ReportViewModel)_currentViewModel;

                if (string.IsNullOrEmpty(reportViewModel.Report))
                {
                    MessageBox.Show("Morate izabrati jedan od izveštaja!",
                        "Izveštaj nije setovan",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                PrinterManager.Instance.PrintReport(reportViewModel.Report);
            }
            else if(_currentViewModel is SaleViewModel)
            {
                SaleViewModel saleViewModel = (SaleViewModel)_currentViewModel;

                DateTime startReport;
                DateTime endReport = DateTime.Now;
                if (endReport.Hour <= 23 && endReport.Hour >= 5)
                {
                    startReport = new DateTime(endReport.Year, endReport.Month, endReport.Day, 5, 0, 0);
                }
                else
                {
                    DateTime end = endReport.AddDays(-1);
                    startReport = new DateTime(end.Year, end.Month, end.Day, 5, 0, 0);
                }

                UniversalEsir_Report.Report report = new UniversalEsir_Report.Report(startReport,
                    endReport,
                    saleViewModel.LoggedCashier);

                string reportString = FormatPos.CreateReport(report);

                PrinterManager.Instance.PrintReport(reportString);
            }
        }
    }
}
