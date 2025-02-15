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

namespace UniversalEsir.Models.AppMain.Statistic.Driver
{
    public class Isporuka : ObservableObject
    {
        private string _id;
        private string _displayName;
        private int _counter;
        private DateTime _createDate;
        private DateTime? _dateIsporuka;
        private decimal _totalAmount;
        private Driver _driver;
        private ObservableCollection<DriverInvoice> _driverInvoices;

        public Isporuka() 
        {
        }
        public Isporuka(IsporukaDB isporukaDB, Driver driver) 
        { 
            Id = isporukaDB.Id;
            Counter = isporukaDB.Counter;
            DisplayName = $"Isporuka_{Counter}";
            CreateDate = isporukaDB.CreateDate;
            DateIsporuka = isporukaDB.DateIsporuka;
            Driver = driver;

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            var invoicesInIsporuka = sqliteDbContext.Invoices.Join(sqliteDbContext.DriverInvoices.Where(di => di.IsporukaId == isporukaDB.Id),
                invoice => invoice.Id,
                driverInvoices => driverInvoices.InvoiceId,
                (invoice, driverInvoices) => new { Invoice = invoice })
                .Select(inv => inv.Invoice);

            if (invoicesInIsporuka.Any())
            {
                invoicesInIsporuka = invoicesInIsporuka.OrderBy(invoice => invoice.SdcDateTime);
            }

            DriverInvoices = new ObservableCollection<DriverInvoice>();
            int index = 1;
            invoicesInIsporuka.ForEachAsync(invoice =>
            {
                Sale.Invoice inv = new Sale.Invoice(invoice, index++);
                DriverInvoices.Add(new DriverInvoice(inv, this));
            });
            TotalAmount = isporukaDB.TotalAmount;
        }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChange(nameof(DisplayName));
            }
        }
        public int Counter
        {
            get { return _counter; }
            set
            {
                _counter = value;
                OnPropertyChange(nameof(Counter));
            }
        }
        public DateTime CreateDate
        {
            get { return _createDate; }
            set
            {
                _createDate = value;
                OnPropertyChange(nameof(CreateDate));
            }
        }
        public DateTime? DateIsporuka
        {
            get { return _dateIsporuka; }
            set
            {
                _dateIsporuka = value;
                OnPropertyChange(nameof(DateIsporuka));
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
        public Driver Driver
        {
            get { return _driver; }
            set
            {
                _driver = value;
                OnPropertyChange(nameof(Driver));
            }
        }
        public ObservableCollection<DriverInvoice> DriverInvoices
        {
            get { return _driverInvoices; }
            set
            {
                _driverInvoices = value;
                OnPropertyChange(nameof(DriverInvoices));
            }
        }
    }
}
