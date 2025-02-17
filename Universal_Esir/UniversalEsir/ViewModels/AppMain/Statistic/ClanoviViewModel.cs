using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UniversalEsir.Commands.AppMain.Clanovi;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir_SportSchedulerAPI;

namespace UniversalEsir.ViewModels.AppMain.Statistic
{
    public class ClanoviViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<Clan> _clanovi;
        private Clan _selectedClan;

        private ObservableCollection<Racun> _racuni;
        private Racun _selectedRacun;

        private ObservableCollection<RacunItem> _racunItems;

        private Clan _currentClan;
        #endregion Fields

        #region Constructors
        public ClanoviViewModel()
        {
            Clanovi = new ObservableCollection<Clan>();
            SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();
            var clanovi = sportSchedulerAPI_Manager.GetUsersAsync().Result;

            if(clanovi == null)
            {
                MessageBox.Show("Greška prilikom učitavanja korisnika!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if(clanovi.Any())
            {
                foreach(var clan in clanovi)
                {
                    Clanovi.Add(new Clan(clan));
                }
            }
        }
        #endregion Constructors

        #region Properties internal
        internal Window CurrentWindow { get; set; }
        #endregion Properties internal

        #region Properties
        public Clan CurrentClan
        {
            get { return _currentClan; }
            set
            {
                _currentClan = value;
                OnPropertyChange(nameof(CurrentClan));
            }
        }
        public ObservableCollection<Clan> Clanovi
        {
            get { return _clanovi; }
            set
            {
                _clanovi = value;
                OnPropertyChange(nameof(Clanovi));
            }
        }
        public Clan SelectedClan
        {
            get { return _selectedClan; }
            set
            {
                _selectedClan = value;
                OnPropertyChange(nameof(SelectedClan));
            }
        }
        public ObservableCollection<Racun> Racuni
        {
            get { return _racuni; }
            set
            {
                _racuni = value;
                OnPropertyChange(nameof(Racuni));
            }
        }
        public Racun SelectedRacun
        {
            get { return _selectedRacun; }
            set
            {
                _selectedRacun = value;
                OnPropertyChange(nameof(SelectedRacun));
            }
        }
        public ObservableCollection<RacunItem> RacunItems
        {
            get { return _racunItems; }
            set
            {
                _racunItems = value;
                OnPropertyChange(nameof(RacunItems));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand EditUserCommand => new EditUserCommand(this);
        public ICommand OpenUserCommand => new OpenUserCommand(this);
        public ICommand AddNewClanCommand => new AddNewClanCommand(this);
        public ICommand SaveClanCommand => new SaveClanCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
