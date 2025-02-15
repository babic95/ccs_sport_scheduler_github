using UniversalEsir.Commands.AppMain.Statistic;
using UniversalEsir.Commands.AppMain.Statistic.Driver;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.Models.Sale;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_eFaktura.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.ViewModels.AppMain.Statistic
{
    public class DriverViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<Driver> _allDrivers;
        private Driver _currentDriver;

        private ObservableCollection<DriverInvoice> _allNedodeljenePorudzbine;
        private DriverInvoice? _currentNedodeljenaPorudzbina;

        private DriverInvoice? _currentPorudzbina;

        private ObservableCollection<Isporuka> _allIsporuke;
        private Isporuka _currentIsporuka;

        private ObservableCollection<ItemInvoice> _itemsInInvoice;
        private Models.Sale.Invoice _currentInvoice;

        private ObservableCollection<DriverInvoice> _neisporucenoDriverInvoices;

        private string _searchText;

        private bool _isAllSelected;

        private DateTime _startDate;
        private DateTime _endDate;

        private decimal _totalAmountIsporukefromDriver;
        #endregion Fields

        #region Constructors
        public DriverViewModel()
        {
            EndDate = DateTime.Now;
            StartDate = new DateTime(EndDate.Year, 1, 1);
            Initialize();
        }
        #endregion Constructors

        #region Properties internal
        internal Window Window { get; set; }
        internal Window WindowIsporuka { get; set; }
        internal Window WindowItemsInInvoice { get; set; }
        internal List<Driver> Drivers { get; set; }
        #endregion Properties internal

        #region Properties
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChange(nameof(StartDate));
            }
        }
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                OnPropertyChange(nameof(EndDate));
                
            }
        }
        public ObservableCollection<Driver> AllDrivers
        {
            get { return _allDrivers; }
            set
            {
                _allDrivers = value;
                OnPropertyChange(nameof(AllDrivers));
            }
        }
        public Driver CurrentDriver
        {
            get { return _currentDriver; }
            set
            {
                _currentDriver = value;
                OnPropertyChange(nameof(CurrentDriver));
            }
        }
        public ObservableCollection<Isporuka> AllIsporuke
        {
            get { return _allIsporuke; }
            set
            {
                _allIsporuke = value;
                OnPropertyChange(nameof(AllIsporuke));
            }
        }
        public Isporuka CurrentIsporuka
        {
            get { return _currentIsporuka; }
            set
            {
                _currentIsporuka = value;
                OnPropertyChange(nameof(CurrentIsporuka));
            }
        }
        public ObservableCollection<DriverInvoice> AllNedodeljenePorudzbine
        {
            get { return _allNedodeljenePorudzbine; }
            set
            {
                _allNedodeljenePorudzbine = value;
                OnPropertyChange(nameof(AllNedodeljenePorudzbine));
            }
        }
        public DriverInvoice? CurrentNedodeljenaPorudzbina
        {
            get { return _currentNedodeljenaPorudzbina; }
            set
            {
                _currentNedodeljenaPorudzbina = value;
                OnPropertyChange(nameof(CurrentNedodeljenaPorudzbina));
            }
        }
        public DriverInvoice? CurrentPorudzbina
        {
            get { return _currentPorudzbina; }
            set
            {
                _currentPorudzbina = value;
                OnPropertyChange(nameof(CurrentPorudzbina));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInInvoice
        {
            get { return _itemsInInvoice; }
            set
            {
                _itemsInInvoice = value;
                OnPropertyChange(nameof(ItemsInInvoice));
            }
        }
        public Models.Sale.Invoice CurrentInvoice
        {
            get { return _currentInvoice; }
            set
            {
                _currentInvoice = value;
                OnPropertyChange(nameof(CurrentInvoice));
            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChange(nameof(SearchText));

                if (string.IsNullOrEmpty(value))
                {
                    AllDrivers = new ObservableCollection<Driver>(Drivers);
                }
                else
                {
                    var searchDrivers = Drivers.Where(driver => driver.DisplayName.Contains(value));

                    AllDrivers = new ObservableCollection<Driver>(searchDrivers);
                }
            }
        }
        public decimal TotalAmountIsporukefromDriver
        {
            get { return _totalAmountIsporukefromDriver; }
            set
            {
                _totalAmountIsporukefromDriver = value;
                OnPropertyChange(nameof(TotalAmountIsporukefromDriver));
            }
        }
        public ObservableCollection<DriverInvoice> NeisporucenoDriverInvoices
        {
            get { return _neisporucenoDriverInvoices; }
            set
            {
                _neisporucenoDriverInvoices = value;
                OnPropertyChange(nameof(NeisporucenoDriverInvoices));
            }
        }


        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                _isAllSelected = value;
                OnPropertyChange(nameof(IsAllSelected));

                if (CurrentIsporuka != null &&
                    CurrentIsporuka.DriverInvoices.Any())
                {
                    if (value)
                    {
                        CurrentIsporuka.DriverInvoices = new ObservableCollection<DriverInvoice>(CurrentIsporuka.DriverInvoices.Select((invoice) => { invoice.IsChecked = true; return invoice; }));
                    }
                    else
                    {
                        CurrentIsporuka.DriverInvoices = new ObservableCollection<DriverInvoice>(CurrentIsporuka.DriverInvoices.Select((invoice) => { invoice.IsChecked = false; return invoice; }));
                    }
                }
            }
        }

        #endregion Properties

        #region Commands
        public ICommand OpenCurrentIsporukaCommand => new OpenCurrentIsporukaCommand(this);
        public ICommand OpenAllIsporukaCommand => new OpenAllIsporukaCommand(this);
        public ICommand OpenIsporukaCommand => new OpenIsporukaCommand(this);
        public ICommand OpenItemsInIsporukaCommand => new OpenItemsInIsporukaCommand(this);
        public ICommand SelectedDateForIsporukaCommand => new SelectedDateForIsporukaCommand(this);
        public ICommand CreateIsporukaCommand => new CreateIsporukaCommand(this);
        public ICommand EditDriverCommand => new EditDriverCommand(this);
        public ICommand AddNewDriverCommand => new AddNewDriverCommand(this);
        public ICommand SaveDriverCommand => new SaveDriverCommand(this);
        public ICommand PrintIsporukaCommand => new PrintIsporukaCommand(this);
        public ICommand SearchCommand => new SearchCommand(this);
        public ICommand PrintAllIsporukeCommand => new PrintAllIsporukeCommand(this);
        public ICommand OpenNesvrstanePorudzbineCommand => new OpenNesvrstanePorudzbineCommand(this);
        public ICommand AddDriverOnInvoiceCommand => new AddDriverOnInvoiceCommand(this);
        public ICommand SelectedDriverCommand => new SelectedDriverCommand(this);
        public ICommand OpenEditIsporukaCommand => new OpenEditIsporukaCommand(this);
        public ICommand EditIsporukaCommand => new EditIsporukaCommand(this);
        public ICommand OpenInsertIsporukaCommand => new OpenInsertIsporukaCommand(this);
        public ICommand InsertIsporukaCommand => new InsertIsporukaCommand(this);
        public ICommand OpenChangeDriverCommand => new OpenChangeDriverCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods

        #region Internal methods
        internal void Initialize()
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            var refundInvoices = sqliteDbContext.Invoices.Where(invoice => invoice.TransactionType == 1 &&
            (invoice.InvoiceType == 0 || invoice.InvoiceType == 5) &&
            !string.IsNullOrEmpty(invoice.InvoiceNumberResult));

            refundInvoices.ForEachAsync(refundInvoice =>
            {
                var invoice = sqliteDbContext.Invoices.FirstOrDefault(invo => invo.InvoiceNumberResult == refundInvoice.ReferentDocumentNumber);

                if(invoice != null)
                {
                    var driverInvoice = sqliteDbContext.DriverInvoices.FirstOrDefault(di => di.InvoiceId == invoice.Id);

                    if(driverInvoice != null)
                    {
                        sqliteDbContext.DriverInvoices.Remove(driverInvoice);
                    }
                }
            });

            sqliteDbContext.SaveChanges();

            Drivers = new List<Driver>();
            AllDrivers = new ObservableCollection<Driver>();
            TotalAmountIsporukefromDriver = 0;

            sqliteDbContext.Drivers.ForEachAsync(driver =>
            {
                bool hasDelevery = sqliteDbContext.DriverInvoices.Any(d => d.DriverId == driver.Id &&
                d.IsporukaId == null);

                Drivers.Add(new Driver(driver, hasDelevery));
            });
            AllDrivers = new ObservableCollection<Driver>(Drivers);
            CurrentDriver = new Driver();
        }
        #endregion Internal methods
    }
}
