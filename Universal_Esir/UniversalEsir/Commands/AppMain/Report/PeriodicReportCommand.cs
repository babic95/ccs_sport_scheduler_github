using UniversalEsir.ViewModels.AppMain;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.ReportWindows;
using UniversalEsir_Printer.PaperFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Report
{
    public class PeriodicReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ReportViewModel _currentViewModel;

        public PeriodicReportCommand(ReportViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.Title = "PERIODIČNI IZVEŠTAJ";

            _currentViewModel.AuxiliaryWindow = new PeriodicreportWindow(_currentViewModel);
            _currentViewModel.AuxiliaryWindow.ShowDialog();

            DateTime start = _currentViewModel.StartReport.Date;
            DateTime end = _currentViewModel.EndReport.Date;

            _currentViewModel.StartReport = new DateTime(start.Year, start.Month, start.Day, 5, 0, 0);
            _currentViewModel.EndReport = new DateTime(end.Year, end.Month, end.Day, 5, 0, 0);

            UniversalEsir_Report.Report report;
            if (string.IsNullOrEmpty(_currentViewModel.SmartCard))
            {
                report = new UniversalEsir_Report.Report(_currentViewModel.StartReport,
                    _currentViewModel.EndReport,
                    _currentViewModel.Items);
            }
            else
            {
                report = new UniversalEsir_Report.Report(_currentViewModel.StartReport,
                    _currentViewModel.EndReport,
                    _currentViewModel.Items,
                    _currentViewModel.SmartCard);
            }

            _currentViewModel.CurrentReport = report;

#if CRNO
            _currentViewModel.Report = FormatPos.CreateReportBlack(report);
#else
            _currentViewModel.Report = FormatPos.CreateReport(report);
#endif
        }
    }
}