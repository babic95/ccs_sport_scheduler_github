using UniversalEsir.Commands.Sale;
using UniversalEsir.Commands.Sale.Pay;
using UniversalEsir.Enums.Sale;
using UniversalEsir.Enums.Sale.Buyer;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.Models.Sale;
using UniversalEsir.Models.Sale.Buyer;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UniversalEsir_Logging;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir_SportSchedulerAPI;

namespace UniversalEsir.ViewModels.Sale
{
    public enum FocusEnumeration
    {
        BuyerId = 0,
        Cash = 1,
        Card = 2,
        Check = 3,
        Voucher = 4,
        Other = 5,
        WireTransfer = 6,
        MobileMoney = 7,
        Pay = 8,
    }
    public class PaySaleViewModel : ViewModelBase
    {
        #region Fields
        private FocusEnumeration _focus;

        private ObservableCollection<ItemInvoice> _itemsInvoice;
        private InvoiceTypeEnumeration _invoiceType;
        private decimal _totalAmount;
        private decimal _amount;
        private decimal _rest;
        private Brush _amountBorderBrush;
        private bool _isEnablePay;

        private Visibility _buyerVisibility;

        private string _cashierNema;
        private string _porudzbenica;
        private string _buyerId; 
        private string _buyerName;
        private string _buyerAdress;
        private string _popust;
        private string _popustFiksan;
        private ObservableCollection<BuyerIdElement> _buyerIdElements;
        private BuyerIdElement _currentBuyerIdElement;
        private ObservableCollection<Partner> _partners;
        private Partner _currentPartner;

        private ObservableCollection<Clan> _clanovi;
        private Clan _currentClan;

        private ObservableCollection<Driver> _allDrivers;
        private Driver _currentDriver;

        private string _cash;
        private string _card;
        private string _check;
        private string _voucher;
        private string _other;
        private string _wireTransfer;
        private string _mobileMoney;

        private Visibility _visibilityBlack;
        private Visibility _visibilityBlackView;

        private string _lastAdvanceInvoice;

        private string _gotovina;
        private bool _firstChangeGotovina;
        #endregion Fields

        #region Constructors
        public PaySaleViewModel(Window window, SaleViewModel saleViewModel)
        {

#if CRNO
            VisibilityBlack = Visibility.Hidden;
#else
            VisibilityBlack = Visibility.Visible;
#endif
            CurrentClan = saleViewModel.CurrentClan;
            Window = window;
            SaleViewModel = saleViewModel;
            CashierNema = saleViewModel.CashierNema;
            BuyerVisibility = Visibility.Hidden;
            SplitOrder = false;

            //Payment = new List<Payment>();

            ItemsInvoice = SaleViewModel.ItemsInvoice;
            TotalAmount = SaleViewModel.TotalAmount;
            InvoiceType = InvoiceTypeEnumeration.Promet;

            BuyerIdElements = new ObservableCollection<BuyerIdElement>();
            var buyerIdElements = Enum.GetValues(typeof(BuyerIdElementEnumeration));

            foreach (var buyerIdElement in buyerIdElements)
            {
                BuyerIdElements.Add(new BuyerIdElement((BuyerIdElementEnumeration)buyerIdElement));
            }
            CurrentBuyerIdElement = BuyerIdElements.FirstOrDefault();

            Partners = new ObservableCollection<Partner>();
            SqliteDbContext sqliteDbContext = new SqliteDbContext();
            sqliteDbContext.Partners.ForEachAsync(partner =>
            {
                Partners.Add(new Partner(partner));
            });
            CurrentPartner = new Partner();

            AllDrivers = new ObservableCollection<Driver>();
            sqliteDbContext.Drivers.ForEachAsync(driver =>
            {
                bool hasDelevery = sqliteDbContext.DriverInvoices.Any(d => d.DriverId == driver.Id &&
                d.IsporukaId == null);

                AllDrivers.Add(new Driver(driver, hasDelevery));
            });
            CurrentDriver = new Driver(); Gotovina = "0";

        }
        #endregion Constructors

        #region Properties

        public string LastAdvanceInvoice
        {
            get { return _lastAdvanceInvoice; }
            set
            {
                _lastAdvanceInvoice = value;
                OnPropertyChange(nameof(LastAdvanceInvoice));
            }
        }
        public Visibility VisibilityBlackView
        {
            get { return _visibilityBlackView; }
            set
            {
                _visibilityBlackView = value;
                OnPropertyChange(nameof(VisibilityBlackView));
            }
        }
        public Visibility VisibilityBlack
        {
            get { return _visibilityBlack; }
            set
            {
                _visibilityBlack = value;
                OnPropertyChange(nameof(VisibilityBlack));
                if(value == Visibility.Visible)
                {
                    VisibilityBlackView = Visibility.Hidden;
                }
                else
                {
                    VisibilityBlackView = Visibility.Visible;
                }
            }
        }
        public bool SplitOrder { get; set; }
        public Visibility BuyerVisibility
        {
            get { return _buyerVisibility; }
            set
            {
                _buyerVisibility = value;
                OnPropertyChange(nameof(BuyerVisibility));
            }
        }
        public FocusEnumeration Focus
        {
            get { return _focus; }
            set
            {
                _focus = value;
                OnPropertyChange(nameof(Focus));
            }
        }
        public string CashierNema
        {
            get { return _cashierNema; }
            set
            {
                _cashierNema = value;
                OnPropertyChange(nameof(CashierNema));
            }
        }
        public bool IsEnablePay
        {
            get { return _isEnablePay; }
            set
            {
                _isEnablePay = value;
                OnPropertyChange(nameof(IsEnablePay));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInvoice
        {
            get { return _itemsInvoice; }
            set
            {
                _itemsInvoice = value;
                OnPropertyChange(nameof(ItemsInvoice));
            }
        }
        public InvoiceTypeEnumeration InvoiceType
        {
            get { return _invoiceType; }
            set
            {
                _invoiceType = value;
                OnPropertyChange(nameof(InvoiceType));
            }
        }
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChange(nameof(TotalAmount));

                AmountBorderBrush = Brushes.Red;

                Cash = value.ToString();// "0";
                Card = "0";
                Check = "0";
                Voucher = "0";
                Other = "0";
                WireTransfer = "0";
                MobileMoney = "0";

                Focus = FocusEnumeration.Cash;

                Rest = value;
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChange(nameof(Amount));

                if (value >= TotalAmount)
                {
                    AmountBorderBrush = Brushes.Transparent;
                    IsEnablePay = true;
                }
                else
                {
                    AmountBorderBrush = Brushes.Red;
                    IsEnablePay = false;
                }
                Rest = TotalAmount - value;
            }
        }
        public decimal Rest
        {
            get { return _rest; }
            set
            {
                _rest = value;
                OnPropertyChange(nameof(Rest));
            }
        }

        public ObservableCollection<Driver> AllDrivers
        {
            get { return _allDrivers; }
            set
            {
                _allDrivers = value;
                OnPropertyChange(nameof(AllDrivers));
            }
        }

        public Driver CurrentDriver
        {
            get { return _currentDriver; }
            set
            {
                _currentDriver = value;
                OnPropertyChange(nameof(CurrentDriver));
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
        public Partner CurrentPartner
        {
            get { return _currentPartner; }
            set
            {
                _currentPartner = value;
                OnPropertyChange(nameof(CurrentPartner));

                if (value != null)
                {
                    BuyerId = value.Pib;
                    BuyerName = value.Name;
                    if (!string.IsNullOrEmpty(value.Address))
                    {
                        BuyerAdress = value.Address;

                        if (!string.IsNullOrEmpty(value.City))
                        {
                            BuyerAdress += $" {value.City}";
                        }
                    }
                }
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
        public Clan CurrentClan
        {
            get { return _currentClan; }
            set
            {
                _currentClan = value;
                OnPropertyChange(nameof(CurrentClan));
            }
        }
        public ObservableCollection<BuyerIdElement> BuyerIdElements
        {
            get { return _buyerIdElements; }
            set
            {
                _buyerIdElements = value;
                OnPropertyChange(nameof(BuyerIdElements));
            }
        }
        public BuyerIdElement CurrentBuyerIdElement
        {
            get { return _currentBuyerIdElement; }
            set
            {
                _currentBuyerIdElement = value;
                OnPropertyChange(nameof(CurrentBuyerIdElement));
            }
        }
        public Brush AmountBorderBrush
        {
            get { return _amountBorderBrush; }
            set
            {
                _amountBorderBrush = value;
                OnPropertyChange(nameof(AmountBorderBrush));
            }
        }
        public string BuyerId
        {
            get { return _buyerId; }
            set
            {
                _buyerId = value;
                OnPropertyChange(nameof(BuyerId));

                if (!string.IsNullOrEmpty(value))
                {
                    BuyerVisibility = Visibility.Visible;
                }
                else
                {
                    BuyerVisibility = Visibility.Hidden;
                }

                if (!string.IsNullOrEmpty(value) &&
                    Amount >= SaleViewModel.TotalAmount)
                {
                    AmountBorderBrush = Brushes.Transparent;
                    IsEnablePay = true;
                }
                else
                {
                    AmountBorderBrush = Brushes.Red;
                    IsEnablePay = false;
                }
            }
        }
        
        public string Porudzbenica
        {
            get { return _porudzbenica; }
            set
            {
                _porudzbenica = value;
                OnPropertyChange(nameof(Porudzbenica));
            }
        }
        public string BuyerName
        {
            get { return _buyerName; }
            set
            {
                _buyerName = value;
                OnPropertyChange(nameof(BuyerName));
            }
        }
        public string BuyerAdress
        {
            get { return _buyerAdress; }
            set
            {
                _buyerAdress = value;
                OnPropertyChange(nameof(BuyerAdress));
            }
        }
        public string Popust
        {
            get { return _popust; }
            set
            {
                _popust = value;
                OnPropertyChange(nameof(Popust));

                if (!string.IsNullOrEmpty(value))
                {
                    PopustFiksan = string.Empty;
                }
            }
        }
        public string PopustFiksan
        {
            get { return _popustFiksan; }
            set
            {
                _popustFiksan = value;
                OnPropertyChange(nameof(PopustFiksan));

                if (!string.IsNullOrEmpty(value))
                {
                    Popust = string.Empty;
                }
            }
        }
        
        public string Cash
        {
            get { return _cash; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _cash.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _cash = value;
                    OnPropertyChange(nameof(Cash));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Gotovina' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Card
        {
            get { return _card; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _card.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _card = value;
                    OnPropertyChange(nameof(Card));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Platna kartica' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Check
        {
            get { return _check; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _check.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _check = value;
                    OnPropertyChange(nameof(Check));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Ček' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Voucher
        {
            get { return _voucher; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _voucher.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _voucher = value;
                    OnPropertyChange(nameof(Voucher));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Vaučer' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Other
        {
            get { return _other; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _other.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _other = value;
                    OnPropertyChange(nameof(Other));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Drugo bezgotovinsko plaćanje' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string WireTransfer
        {
            get { return _wireTransfer; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _wireTransfer.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _wireTransfer = value;
                    OnPropertyChange(nameof(WireTransfer));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Prenos na račun' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string MobileMoney
        {
            get { return _mobileMoney; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Convert.ToDecimal(value);

                        if (value[value.Length - 1] == '.' || value[value.Length - 1] == ',')
                        {
                            if (value.Length > _mobileMoney.Length)
                            {
                                value += "0";
                            }
                            else
                            {
                                value = value.Remove(value.Length - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        value = "0";
                    }

                    _mobileMoney = value;
                    OnPropertyChange(nameof(MobileMoney));

                    if (Amount >= SaleViewModel.TotalAmount)
                    {
                        AmountBorderBrush = Brushes.Transparent;
                        IsEnablePay = true;
                        Focus = FocusEnumeration.Pay;
                    }
                }
                catch
                {
                    MessageBox.Show("Polje 'Instant plaćanje' mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public string Gotovina
        {
            get { return _gotovina; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Contains(","))
                    {
                        value = value.Replace(",", ".");
                    }

                    try
                    {
                        var quantity = Decimal.Round(Convert.ToDecimal(value), 2);
                        value = quantity.ToString();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"PaySaleViewModel -> Gotovina Property -> gotovina mora biti broj!", ex);
                        MessageBox.Show("Gotovina mora biti broj",
                            "Greška u Gotovini",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        value = "0";
                    }
                }

                _gotovina = value;
                OnPropertyChange(nameof(Gotovina));
            }
        }
        public bool FirstChangeGotovina
        {
            get { return _firstChangeGotovina; }
            set
            {
                _firstChangeGotovina = value;
                OnPropertyChange(nameof(FirstChangeGotovina));
                if (value)
                {
                    Gotovina = "0";
                }
            }
        }
        #endregion Properties

        #region Internal Properties
        internal Payment Payment { get; set; }
        internal SaleViewModel SaleViewModel { get; set; }
        internal Window Window { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand CancelCommand => new CancelCommand(this);
        public ICommand ClickOnNumberButtonCommand => new ClickOnNumberButtonCommand(this);
        public ICommand PayCommand => new PayCommand(this);
        public ICommand SplitOrderCommand => new SplitOrderCommand(this);
        public ICommand ChangeFocusCommand => new ChangeFocusCommand(this);
        public ICommand ClickOnGotovinaButtonCommand => new ClickOnGotovinaButtonCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}