﻿using UniversalEsir.Commands.AppMain.Statistic;
using UniversalEsir.Commands.AppMain.Statistic.Calculation;
using UniversalEsir.Commands.AppMain.Statistic.Knjizenje;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic.Knjizenje;
using UniversalEsir.Models.Sale;
using UniversalEsir_Database;
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
    public class KnjizenjeViewModel : ViewModelBase
    {
        #region Fields
        private DateTime _currentDate;
        private ObservableCollection<Invoice> _invoices;
        private KnjizenjePazara _currentKnjizenjePazara;
        private ObservableCollection<ItemInvoice> _itemsInInvoice;
        #endregion Fields

        #region Constructors
        public KnjizenjeViewModel()
        {
            CurrentDate = DateTime.Now;
            Invoices = new ObservableCollection<Invoice>();

            SearchInvoicesCommand.Execute(null);
        }
        #endregion Constructors

        #region Properties internal
        internal Window Window { get; set; }
        #endregion Properties internal

        #region Properties
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChange(nameof(CurrentDate));
            }
        }
        public ObservableCollection<Invoice> Invoices
        {
            get { return _invoices; }
            set
            {
                _invoices = value;
                OnPropertyChange(nameof(Invoices));
            }
        }
        public KnjizenjePazara CurrentKnjizenjePazara
        {
            get { return _currentKnjizenjePazara; }
            set
            {
                _currentKnjizenjePazara = value;
                OnPropertyChange(nameof(CurrentKnjizenjePazara));
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
        #endregion Properties

        #region Commands
        public ICommand SearchInvoicesCommand => new SearchInvoicesCommand(this);
        public ICommand OpenItemsInInvoicesCommand => new OpenItemsInInvoicesCommand(this);
        public ICommand KnjizenjeCommand => new KnjizenjeCommand(this);
        public ICommand CloseWindowCommand => new CloseWindowCommand(this);
        public ICommand PrintDnevniPazarCommand => new PrintDnevniPazarCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
