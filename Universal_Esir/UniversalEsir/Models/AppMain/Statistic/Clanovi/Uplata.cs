﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Uplate;

namespace UniversalEsir.Models.AppMain.Statistic.Clanovi
{
    public class Uplata : ObservableObject
    {
        private string _id;
        private int _userId;
        private decimal _totalAmount;
        private DateTime _date;
        private UplataEnumeration _typeUplata;
        private string _description;
        private string _totalAmountString;

        public Uplata() { }
        public Uplata(UplataResponse uplata)
        {
            Id = uplata.Id;
            UserId = uplata.UserId;
            TotalAmount = uplata.TotalAmount;
            Date = uplata.Date;
            TypeUplata = (UplataEnumeration)uplata.TypeUplata;
            Description = uplata.Description;
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
        public UplataEnumeration TypeUplata
        {
            get { return _typeUplata; }
            set
            {
                _typeUplata = value;
                OnPropertyChange(nameof(TypeUplata));
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChange(nameof(Description));
            }
        }
        public string TotalAmountString
        {
            get { return _totalAmountString; }
            set
            {
                if(value.Contains(","))
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
    }
}
