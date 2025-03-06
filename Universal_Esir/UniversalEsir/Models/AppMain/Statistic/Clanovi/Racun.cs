using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Racuni;

namespace UniversalEsir.Models.AppMain.Statistic.Clanovi
{
    public class Racun : ObservableObject
    {
        private string _id;
        private int _userId;
        private string _invoiceNumber;
        private DateTime _date;
        private decimal _totalAmount;
        private decimal _placeno;
        private decimal _otpis;

        public Racun() { }
        public Racun(RacunResponse racun)
        {
            Id = racun.Id;
            UserId = racun.UserId;
            InvoiceNumber = racun.InvoiceNumber;
            Date = racun.Date;
            TotalAmount = racun.TotalAmount;
            Placeno = racun.Placeno;
            Otpis = racun.Otpis;
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
        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChange(nameof(UserId));
            }
        }
        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set
            {
                _invoiceNumber = value;
                OnPropertyChange(nameof(InvoiceNumber));
            }
        }
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChange(nameof(Date));
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
        public decimal Placeno
        {
            get { return _placeno; }
            set
            {
                _placeno = value;
                OnPropertyChange(nameof(Placeno));
            }
        }
        public decimal Otpis
        {
            get { return _otpis; }
            set
            {
                _otpis = value;
                OnPropertyChange(nameof(Otpis));
            }
        }
    }
}
