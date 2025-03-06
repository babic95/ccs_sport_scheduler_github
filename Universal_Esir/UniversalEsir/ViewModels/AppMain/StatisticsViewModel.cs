using UniversalEsir.Commands.AppMain;
using UniversalEsir.Commands.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.ViewModels.Sale;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.ViewModels.AppMain
{
    public class StatisticsViewModel : ViewModelBase
    {
        #region Fields
        private CashierDB _loggedCashier;
        private AppMainViewModel _mainViewModel;
        private ViewModelBase _currentViewModel;
        
        private bool _isCheckedKnjizenje;
        private bool _isCheckedKEP;
        private bool _isCheckedRefaund;
        private bool _isCheckedInventoryStatus;
        private bool _isCheckedClanovi;
        private bool _isCheckedAddEditSupplier;
        private bool _isCheckedCalculation;
        private bool _isCheckedViewCalculation; 
        private bool _isCheckedFirma;
        private bool _isCheckedPriceIncrease;
        private bool _isCheckedNivelacija;
        private bool _isCheckedNivelacijaView;
        private bool _isCheckedViewKnjizenje;
        private bool _isCheckedNorm;
        private bool _isCheckedPartner;
        private bool _isCheckedDriver;
        private bool _isCheckedOtpremnice; 
        #endregion Fields

        #region Constructors
        public StatisticsViewModel(CashierDB loggedCashier, AppMainViewModel mainViewModel)
        {
            _loggedCashier = loggedCashier;
            _mainViewModel = mainViewModel;
            //CheckedInventoryStatus();
        }
        #endregion Constructors

        #region Properties
        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                OnPropertyChange(nameof(CurrentViewModel));
            }
        }
        
        public bool IsCheckedViewKnjizenje
        {
            get { return _isCheckedViewKnjizenje; }
            set
            {
                _isCheckedViewKnjizenje = value;
                OnPropertyChange(nameof(IsCheckedViewKnjizenje));
            }
        }
        public bool IsCheckedKnjizenje
        {
            get { return _isCheckedKnjizenje; }
            set
            {
                _isCheckedKnjizenje = value;
                OnPropertyChange(nameof(IsCheckedKnjizenje));
            }
        }
        public bool IsCheckedRefaund
        {
            get { return _isCheckedRefaund; }
            set
            {
                _isCheckedRefaund = value;
                OnPropertyChange(nameof(IsCheckedRefaund));
            }
        }
        public bool IsCheckedKEP
        {
            get { return _isCheckedKEP; }
            set
            {
                _isCheckedKEP = value;
                OnPropertyChange(nameof(IsCheckedKEP));
            }
        }
        public bool IsCheckedDriver
        {
            get { return _isCheckedDriver; }
            set
            {
                _isCheckedDriver = value;
                OnPropertyChange(nameof(IsCheckedDriver));
            }
        }
        public bool IsCheckedOtpremnice
        {
            get { return _isCheckedOtpremnice; }
            set
            {
                _isCheckedOtpremnice = value;
                OnPropertyChange(nameof(IsCheckedOtpremnice));
            }
        }
        public bool IsCheckedPartner
        {
            get { return _isCheckedPartner; }
            set
            {
                _isCheckedPartner = value;
                OnPropertyChange(nameof(IsCheckedPartner));
            }
        }
        public bool IsCheckedInventoryStatus
        {
            get { return _isCheckedInventoryStatus; }
            set
            {
                _isCheckedInventoryStatus = value;
                OnPropertyChange(nameof(IsCheckedInventoryStatus));
            }
        }
        public bool IsCheckedClanovi
        {
            get { return _isCheckedClanovi; }
            set
            {
                _isCheckedClanovi = value;
                OnPropertyChange(nameof(IsCheckedClanovi));
            }
        }
        public bool IsCheckedViewCalculation
        {
            get { return _isCheckedViewCalculation; }
            set
            {
                _isCheckedViewCalculation = value;
                OnPropertyChange(nameof(IsCheckedViewCalculation));
            }
        }
        public bool IsCheckedAddEditSupplier
        {
            get { return _isCheckedAddEditSupplier; }
            set
            {
                _isCheckedAddEditSupplier = value;
                OnPropertyChange(nameof(IsCheckedAddEditSupplier));
            }
        }
        public bool IsCheckedCalculation
        {
            get { return _isCheckedCalculation; }
            set
            {
                _isCheckedCalculation = value;
                OnPropertyChange(nameof(IsCheckedCalculation));
            }
        }
        
        public bool IsCheckedNorm
        {
            get { return _isCheckedNorm; }
            set
            {
                _isCheckedNorm = value;
                OnPropertyChange(nameof(IsCheckedNorm));
            }
        }
        
        public bool IsCheckedFirma
        {
            get { return _isCheckedFirma; }
            set
            {
                _isCheckedFirma = value;
                OnPropertyChange(nameof(IsCheckedFirma));
            }
        }
        public bool IsCheckedPriceIncrease
        {
            get { return _isCheckedPriceIncrease; }
            set
            {
                _isCheckedPriceIncrease = value;
                OnPropertyChange(nameof(IsCheckedPriceIncrease));
            }
        }

        public bool IsCheckedNivelacija
        {
            get { return _isCheckedNivelacija; }
            set
            {
                _isCheckedNivelacija = value;
                OnPropertyChange(nameof(IsCheckedNivelacija));
            }
        }
        public bool IsCheckedNivelacijaView
        {
            get { return _isCheckedNivelacijaView; }
            set
            {
                _isCheckedNivelacijaView = value;
                OnPropertyChange(nameof(IsCheckedNivelacijaView));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentViewModelCommand(_mainViewModel);
        public ICommand UpdateCurrentStatisticViewModelCommand => new UpdateCurrentStatisticViewModelCommand(this);
        #endregion Commands

        #region Public methods
        public void CheckedInventoryStatus()
        {
            CurrentViewModel = new InventoryStatusViewModel();
            IsCheckedInventoryStatus = true;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedAddEditSupplier()
        {
            CurrentViewModel = new AddEditSupplierViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = true;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedCalculation()
        {
            CurrentViewModel = new CalculationViewModel(_loggedCashier);
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = true;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedViewCalculation()
        {
            CurrentViewModel = new ViewCalculationViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = true;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedPriceIncrease()
        {
            CurrentViewModel = new PriceIncreaseViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = true;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedNivelacija()
        {
            CurrentViewModel = new NivelacijaViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = true;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedViewNivelacija()
        {
            CurrentViewModel = new ViewNivelacijaViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = true;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedKnjizenje()
        {
            CurrentViewModel = new KnjizenjeViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = true;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedKEP()
        {
            CurrentViewModel = new KEPViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = true;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedClanovi = false;
            IsCheckedOtpremnice = false;
        }
        public void CheckedViewKnjizenje()
        {
            CurrentViewModel = new PregledPazaraViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = true;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedRefaund()
        {
            CurrentViewModel = new RefaundViewModel(_loggedCashier, _mainViewModel);
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = true;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedNorm()
        {
            CurrentViewModel = new NormViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = true;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedFirma()
        {
            CurrentViewModel = new FirmaViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = true;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedPartner()
        {
            CurrentViewModel = new PartnerViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = true;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedDriver()
        {
            CurrentViewModel = new DriverViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = true;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = false;
        }
        public void CheckedOtpremnice()
        {
            CurrentViewModel = new OtpremniceViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = true;
            IsCheckedClanovi = false;
        }
        public void CheckedClanovi()
        {
            CurrentViewModel = new ClanoviViewModel();
            IsCheckedInventoryStatus = false;
            IsCheckedAddEditSupplier = false;
            IsCheckedCalculation = false;
            IsCheckedViewCalculation = false;
            IsCheckedPriceIncrease = false;
            IsCheckedNivelacija = false;
            IsCheckedNivelacijaView = false;
            IsCheckedKnjizenje = false;
            IsCheckedKEP = false;
            IsCheckedViewKnjizenje = false;
            IsCheckedRefaund = false;
            IsCheckedNorm = false;
            IsCheckedFirma = false;
            IsCheckedPartner = false;
            IsCheckedDriver = false;
            IsCheckedOtpremnice = false;
            IsCheckedClanovi = true;
        }

        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
