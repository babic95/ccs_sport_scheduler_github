﻿using UniversalEsir.Commands.AppMain.Statistic.Parner;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
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
    public class PartnerViewModel : ViewModelBase
    {
        #region Fields
        private Partner _currentPartner;
        private ObservableCollection<Partner> _partners;

        private string _searchText;

        private string _title;
        #endregion Fields

        #region Constructors
        public PartnerViewModel()
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            sqliteDbContext.Partners.ToList().ForEach(x =>
            {
                PartnersAll.Add(new Partner(x));
            });

            Partners = new ObservableCollection<Partner>(PartnersAll);
            CurrentPartner = new Partner();
        }
        #endregion Constructors

        #region Properties internal
        internal List<Partner> PartnersAll = new List<Partner>();
        internal Window Window { get; set; }
        #endregion Properties internal

        #region Properties
        public Partner CurrentPartner
        {
            get { return _currentPartner; }
            set
            {
                _currentPartner = value;
                OnPropertyChange(nameof(CurrentPartner));
            }
        }
        public ObservableCollection<Partner> Partners
        {
            get { return _partners; }
            set
            {
                _partners = value;
                OnPropertyChange(nameof(Partners));
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
                    Partners = new ObservableCollection<Partner>(PartnersAll);
                }
                else
                {
                    Partners = new ObservableCollection<Partner>(PartnersAll.Where(Partner => Partner.Name.ToLower().Contains(value.ToLower())));
                    if (!Partners.Any())
                    {
                        Partners = new ObservableCollection<Partner>(PartnersAll.Where(Partner => Partner.Pib.Contains(value)));
                    }
                }
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
        #endregion Properties

        #region Commands
        public ICommand DeleteCommand => new DeletePartnerCommand(this);
        public ICommand EditCommand => new EditPartnerCommand(this);
        public ICommand OpenAddEditWindow => new OpenAddEditPartnerWindow(this);
        public ICommand SaveCommand => new SavePartnerCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
