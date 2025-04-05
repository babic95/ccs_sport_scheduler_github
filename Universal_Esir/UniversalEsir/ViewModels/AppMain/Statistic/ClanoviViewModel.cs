using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UniversalEsir.Commands.AppMain.Clanovi;
using UniversalEsir.Commands.AppMain.Statistic.Clanovi;
using UniversalEsir.Commands.AppMain.Tereni;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir.Models.AppMain.Statistic.Tereni;
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
        private ObservableCollection<SatiTermina> _satiTermina;
        private SatiTermina _currentSatiTermina;
        private DanNedeljeEnumeration _currentDan;

        private Clan _currentClan;
        private Teren _currentTeren;
        private Uplata _currentUplata;
        private Zaduzenje _currentZaduzenje;
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
                int rb = 1;
                foreach(var clan in clanovi)
                {
                    Clanovi.Add(new Clan(clan, rb++));
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
        public Teren CurrentTeren
        {
            get { return _currentTeren; }
            set
            {
                _currentTeren = value;
                OnPropertyChange(nameof(CurrentTeren));
            }
        }
        public Uplata CurrentUplata
        {
            get { return _currentUplata; }
            set
            {
                _currentUplata = value;
                OnPropertyChange(nameof(CurrentUplata));
            }
        }
        public Zaduzenje CurrentZaduzenje
        {
            get { return _currentZaduzenje; }
            set
            {
                _currentZaduzenje = value;
                OnPropertyChange(nameof(CurrentZaduzenje));
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
        public ObservableCollection<SatiTermina> SatiTermina
        {
            get { return _satiTermina; }
            set
            {
                _satiTermina = value;
                OnPropertyChange(nameof(SatiTermina));
            }
        }
        public SatiTermina CurrentSatiTermina
        {
            get { return _currentSatiTermina; }
            set
            {
                _currentSatiTermina = value;
                OnPropertyChange(nameof(CurrentSatiTermina));
            }
        }
        public DanNedeljeEnumeration CurrentDan
        {
            get { return _currentDan; }
            set
            {
                _currentDan = value;
                OnPropertyChange(nameof(CurrentDan));
            }
        }

        #endregion Properties

        #region Commands
        public ICommand EditUserCommand => new EditUserCommand(this);
        public ICommand OpenUserCommand => new OpenUserCommand(this);
        public ICommand AddNewClanCommand => new AddNewClanCommand(this);
        public ICommand SaveClanCommand => new SaveClanCommand(this);
        public ICommand AddNewTerenCommand => new AddNewTerenCommand(this);
        public ICommand SaveTerenCommand => new SaveTerenCommand(this);
        public ICommand AddNewUplataCommand => new AddNewUplataCommand(this);
        public ICommand SaveUplataCommand => new SaveUplataCommand(this);
        public ICommand AddNewOtpisCommand => new AddNewOtpisCommand(this);
        public ICommand SaveOtpisCommand => new SaveOtpisCommand(this);
        public ICommand AddNewPozajmicaCommand => new AddNewPozajmicaCommand(this);
        public ICommand SavePozajmicaCommand => new SavePozajmicaCommand(this);
        public ICommand AddNewClanarinaCommand => new AddNewClanarinaCommand(this);
        public ICommand SaveClanarinaCommand => new SaveClanarinaCommand(this);
        public ICommand AddNewPozajmicaIsplataCommand => new AddNewPozajmicaIsplataCommand(this);
        public ICommand SavePozajmicaIsplataCommand => new SavePozajmicaIsplataCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
