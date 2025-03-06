using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Uplate;

namespace UniversalEsir.Models.AppMain.Statistic.Clanovi
{
    public class Zaduzenje : ObservableObject
    {
        private string _id;
        private int _userId;
        private decimal _totalAmount;
        private DateTime _date;
        private ClanEnumeration _clanType;
        private ZaduzenjeEnumeration _typeZaduzenja;
        private string _opis;
        private string _totalAmountString;
        private Visibility _fiksnoVisibility;
        private int? _teren;

        public Zaduzenje() { }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public int? Teren
        {
            get { return _teren; }
            set
            {
                _teren = value;
                OnPropertyChange(nameof(Teren));
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
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChange(nameof(TotalAmount));
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
        public ClanEnumeration ClanType
        {
            get { return _clanType; }
            set
            {
                _clanType = value;
                OnPropertyChange(nameof(ClanType));

                if(value == ClanEnumeration.Fiksni ||
                    value == ClanEnumeration.Trenerski)
                {
                    FiksnoVisibility = Visibility.Visible;
                }
                else
                {
                    FiksnoVisibility = Visibility.Collapsed;
                }
            }
        }
        public ZaduzenjeEnumeration TypeZaduzenja
        {
            get { return _typeZaduzenja; }
            set
            {
                _typeZaduzenja = value;
                OnPropertyChange(nameof(TypeZaduzenja));
            }
        }
        public string Opis
        {
            get { return _opis; }
            set
            {
                _opis = value;
                OnPropertyChange(nameof(Opis));
            }
        }
        public string TotalAmountString
        {
            get { return _totalAmountString; }
            set
            {
                if (value.Contains(","))
                {
                    value = value.Replace(",", ".");
                }

                _totalAmountString = value;
                OnPropertyChange(nameof(TotalAmountString));

                if (!string.IsNullOrEmpty(value))
                {
                    TotalAmount = Convert.ToDecimal(value);
                }
            }
        }
        public Visibility FiksnoVisibility
        {
            get { return _fiksnoVisibility; }
            set
            {
                _fiksnoVisibility = value;
                OnPropertyChange(nameof(FiksnoVisibility));
            }
        }
    }
}
