using UniversalEsir.Commands.AppMain.Statistic.Otpremnice;
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
    public class OtpremniceViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<Models.Sale.Invoice> _allOtpremnice;
        private Models.Sale.Invoice? _currentOtpremnice;

        private ObservableCollection<ItemInvoice> _allItemsInOtpremnica;

        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _totalAmount;

        private string _searchText;
        #endregion Fields

        #region Constructors
        public OtpremniceViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Properties internal
        internal Window Window { get; set; }
        internal List<Models.Sale.Invoice> Otpremnice { get; set; }
        #endregion Properties internal

        #region Properties
        public ObservableCollection<Models.Sale.Invoice> AllOtpremnice
        {
            get { return _allOtpremnice; }
            set
            {
                _allOtpremnice = value;
                OnPropertyChange(nameof(AllOtpremnice));
            }
        }
        public Models.Sale.Invoice? CurrentOtpremnice
        {
            get { return _currentOtpremnice; }
            set
            {
                _currentOtpremnice = value;
                OnPropertyChange(nameof(CurrentOtpremnice));
            }
        }
        public ObservableCollection<ItemInvoice> AllItemsInOtpremnica
        {
            get { return _allItemsInOtpremnica; }
            set
            {
                _allItemsInOtpremnica = value;
                OnPropertyChange(nameof(AllItemsInOtpremnica));
            }
        }
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
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChange(nameof(TotalAmount));
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
                    AllOtpremnice = new ObservableCollection<Models.Sale.Invoice>(Otpremnice);
                }
                else
                {
                    AllOtpremnice = new ObservableCollection<Models.Sale.Invoice>(Otpremnice.Where(otp => otp.Porudzbenica != null &&
                    otp.Porudzbenica.ToLower().Contains(value.ToLower())));
                }
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SearchCommand => new SearchCommand(this);
        public ICommand PrintOtpremnicaCommand => new PrintOtpremnicaCommand(this);
        public ICommand OpenItemsInOtpremnicaCommand => new OpenItemsInOtpremnicaCommand(this);
        public ICommand RefundOtpremnicaCommand => new RefundOtpremnicaCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods

        #region Internal methods
        internal void Initialize()
        {
            TotalAmount = 0;
            EndDate = DateTime.Now;
            StartDate = new DateTime(EndDate.Year, 1, 1);

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            var otpremniceRefund = sqliteDbContext.Invoices.Where(otpremnica => otpremnica.InvoiceType == 5 && 
            otpremnica.TransactionType == 1 &&
            !string.IsNullOrEmpty(otpremnica.InvoiceNumberResult));

            otpremniceRefund.ForEachAsync(refundOtpremnica =>
            {
                var invoice = sqliteDbContext.Invoices.FirstOrDefault(invo => invo.InvoiceNumberResult == refundOtpremnica.ReferentDocumentNumber);

                if (invoice != null)
                {
                    var driverInvoice = sqliteDbContext.DriverInvoices.FirstOrDefault(di => di.InvoiceId == invoice.Id);

                    if (driverInvoice != null)
                    {
                        sqliteDbContext.DriverInvoices.Remove(driverInvoice);
                    }
                }
            });

            sqliteDbContext.SaveChanges();

            Otpremnice = new List<Models.Sale.Invoice>();

            var otpremnice = sqliteDbContext.Invoices.Where(otpremnica => otpremnica.InvoiceType == 5 &&
            otpremnica.TransactionType == 0 &&
            !string.IsNullOrEmpty(otpremnica.InvoiceNumberResult) &&
            otpremnica.SdcDateTime.HasValue &&
            otpremnica.SdcDateTime.Value.Date >= StartDate.Date &&
            otpremnica.SdcDateTime.Value.Date <= EndDate.Date);

            int index = 1;
            if(otpremnice != null &&
                otpremnice.Any())
            {
                otpremnice.ForEachAsync(otpremnicaDB =>
                {
                    if (otpremnicaDB.TotalAmount.HasValue)
                    {
                        var refundOtpremnica = otpremniceRefund.FirstOrDefault(otpremnica =>
                        otpremnica.ReferentDocumentNumber == otpremnicaDB.InvoiceNumberResult);

                        if(refundOtpremnica == null)
                        {
                            Models.Sale.Invoice otpremnica = new Models.Sale.Invoice(otpremnicaDB, index++);
                            Otpremnice.Add(otpremnica);

                            TotalAmount += otpremnicaDB.TotalAmount.Value;
                        }
                    }
                });
            }

            AllOtpremnice = new ObservableCollection<Models.Sale.Invoice>(Otpremnice);
        }
        #endregion Internal methods
    }
}
