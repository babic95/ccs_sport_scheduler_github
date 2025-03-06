using UniversalEsir.Commands.AppMain.Report;
using UniversalEsir_Database.Models;
using UniversalEsir_Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.ViewModels.AppMain
{
    public class ReportViewModel : ViewModelBase
    {
        #region Fields
        private CashierDB _loggedCashier;

        private string _report;
        private string _title;

        private bool _items;
        private string _smartCard;
        #endregion Fields

        #region Constructors
        public ReportViewModel(CashierDB loggedCashier)
        {
            _loggedCashier = loggedCashier;
        }
        #endregion Constructors

        #region Properties
        public Window AuxiliaryWindow { get; set; }
        public DateTime StartReport { get; set; }
        public DateTime EndReport { get; set; }
        public Report CurrentReport { get; set; }
        public string Report
        {
            get { return _report; }
            set
            {
                _report = value;
                OnPropertyChange(nameof(Report));
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChange(nameof(Title));
            }
        }
        public bool Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
        public string SmartCard
        {
            get { return _smartCard; }
            set
            {
                _smartCard = value;
                OnPropertyChange(nameof(SmartCard));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand ReportCommand => new ReportCommand(this);
        public ICommand DayReportCommand => new DayReportCommand(this);
        public ICommand PeriodicReportCommand => new PeriodicReportCommand(this);
        public ICommand PrintReportCommand => new PrintReportCommand(this);
        public ICommand SetDateCommand => new SetDateCommand(this);
        //public ICommand ExportReportCommand => new ExportReportCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
