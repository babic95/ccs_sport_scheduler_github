using UniversalEsir.Commands.AppMain.Report;
using UniversalEsir.Commands.Login;
using UniversalEsir.Commands.Sale;
using UniversalEsir.Models.Sale;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Settings;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using UniversalEsir.Commands.Sale.Pay;

namespace UniversalEsir.ViewModels
{
    public class SaleViewModel : ViewModelBase
    {
        #region Fields
        private AppMainViewModel _mainViewModel;
        private string _cashierNema;

        private Visibility _tableOverviewVisibility;
        private bool _hookOrderEnable;

        private Visibility _visibilitySupergroups;

        private Supergroup _currentSupergroup;
        private GroupItems _currentGroup;
        private ObservableCollection<Supergroup> _supergroups;
        private ObservableCollection<GroupItems> _groups;
        private ObservableCollection<Item> _items;

        private ObservableCollection<ItemInvoice> _itemsInvoice;

        private decimal _totalAmount;
        private int _tableId;

        private Timer _timer;
        private string _currentDateTime;

        private Order _currentOrder;

        private Visibility _visibilityBlack;
        private bool _isEnabledRemoveOrder;

        private SerialPort? _serialPort = null;

        private string _barkod;
        private string _quantity;
        private bool _firstChangeQuantity;

        private string _sifra;
        private string _popust;
        private ItemDB? _currentItem;
        private ObservableCollection<ItemDB> _itemsSearch;
        private string _searchNameText;

        private string _focusedTextBox;
        #endregion Fields

        #region Constructors
        public SaleViewModel(CashierDB loggedCashier, 
            ICommand updateCurrentViewModelCommand)
        {
            var comPort = SettingsManager.Instance.GetComPort();

            if (!string.IsNullOrEmpty(comPort))
            {
                _serialPort = new SerialPort(comPort, 9600);
                _serialPort.WriteTimeout = 500;
            }

            LoggedCashier = loggedCashier;
            CashierNema = loggedCashier.Name;
            UpdateAppViewModelCommand = updateCurrentViewModelCommand;

            Supergroups = new ObservableCollection<Supergroup>();
            Groups = new ObservableCollection<GroupItems>();
            Items = new ObservableCollection<Item>();

            SqliteDbContext sqliteDbContext = new SqliteDbContext();
            AllItems = sqliteDbContext.Items.ToList();
            AllGroups = sqliteDbContext.ItemGroups.ToList();

            if (SettingsManager.Instance.EnableSuperGroup())
            {
                var supergroups = sqliteDbContext.Supergroups.ToList();

                supergroups.ForEach(supergroup =>
                {
                    Supergroup s = new Supergroup(supergroup.Id, supergroup.Name);

                    if (!s.Name.ToLower().Contains("osnovna"))
                    {
                        Supergroups.Add(s);
                    }
                });

                VisibilitySupergroups = Visibility.Visible;
            }
            else
            {
                AllGroups.ForEach(group =>
                {
                    GroupItems g = new GroupItems(group.Id, group.IdSupergroup, group.Name);

                    if (!g.Name.ToLower().Contains("sirovine") &&
                    !g.Name.ToLower().Contains("sirovina"))
                    {
                        Groups.Add(g);
                    }
                });

                VisibilitySupergroups = Visibility.Hidden;
            }

            ItemsInvoice = new ObservableCollection<ItemInvoice>();
            TotalAmount = 0;

            RunTimer();

            TableOverviewViewModel = new TableOverviewViewModel(this);

            if (SettingsManager.Instance.EnableTableOverview())
            {
                TableOverviewVisibility = Visibility.Visible;
            }
            else
            {
                TableOverviewVisibility = Visibility.Hidden;
            }

            if(SettingsManager.Instance.CancelOrderFromTable())
            {
                IsEnabledRemoveOrder = true;
            }
            else
            {
                IsEnabledRemoveOrder = false;
            }
            FirstChangeQuantity = true;

            Sifra = string.Empty;
            Quantity = "1";
            Popust = "0";
            SearchNameText = string.Empty;
        }
        #endregion Constructors

        #region Internal Properties
        internal List<ItemGroupDB> AllGroups { get; set; }
        internal List<ItemDB> AllItems { get; set; }
        internal CashierDB LoggedCashier { get; set; }
        internal Window CurrentWindow { get; private set; }
        #endregion Internal Properties

        #region Properties
        public string FocusedTextBox
        {
            get { return _focusedTextBox; }
            set
            {
                if (_focusedTextBox != value)
                {
                    _focusedTextBox = value;
                    OnPropertyChange(nameof(FocusedTextBox));
                }
            }
        }

        public ItemDB? CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                OnPropertyChange(nameof(CurrentItem));

                if(value != null &&
                    value.Id != Sifra)
                {
                    Sifra = value.Id;
                }
            }
        }
        public ObservableCollection<ItemDB> ItemsSearch
        {
            get { return _itemsSearch; }
            set
            {
                _itemsSearch = value;
                OnPropertyChange(nameof(ItemsSearch));
            }
        }
        public string SearchNameText
        {
            get { return _searchNameText; }
            set
            {
                if(CurrentItem != null &&
                    _searchNameText != value)
                {
                    CurrentItem = null;
                }

                _searchNameText = value;
                OnPropertyChange(nameof(SearchNameText));

                if (string.IsNullOrEmpty(value))
                {
                    ItemsSearch = new ObservableCollection<ItemDB>(AllItems);
                    CurrentItem = null;
                }
                else
                {
                    if (CurrentItem == null)
                    {
                        ItemsSearch = new ObservableCollection<ItemDB>(AllItems.Where(item =>
                        item.Name.ToLower().Contains(value.ToLower())));
                    }
                    else
                    {
                        ItemsSearch = new ObservableCollection<ItemDB>(AllItems.Where(item =>
                        item.Name.ToLower().Contains(value.ToLower()) && item.Id == CurrentItem.Id));
                        
                    }
                }
            }
        }
        public TableOverviewViewModel TableOverviewViewModel { get; set; }
        public Order CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                _currentOrder = value;
                OnPropertyChange(nameof(CurrentOrder));
            }
        }
        public string CurrentDateTime
        {
            get { return _currentDateTime; }
            set
            {
                _currentDateTime = value;
                OnPropertyChange(nameof(CurrentDateTime));
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
        public bool IsEnabledRemoveOrder
        {
            get { return _isEnabledRemoveOrder; }
            set
            {
                _isEnabledRemoveOrder = value;
                OnPropertyChange(nameof(IsEnabledRemoveOrder));
            }
        }
        public Visibility VisibilitySupergroups
        {
            get { return _visibilitySupergroups; }
            set
            {
                _visibilitySupergroups = value;
                OnPropertyChange(nameof(VisibilitySupergroups));
            }
        }
        public Visibility TableOverviewVisibility
        {
            get { return _tableOverviewVisibility; }
            set
            {
                _tableOverviewVisibility = value;
                OnPropertyChange(nameof(TableOverviewVisibility));
            }
        }
        public bool HookOrderEnable
        {
            get { return _hookOrderEnable; }
            set
            {
                _hookOrderEnable = value;
                OnPropertyChange(nameof(HookOrderEnable));
            }
        }

        public Supergroup CurrentSupergroup
        {
            get { return _currentSupergroup; }
            set
            {
                _currentSupergroup = value;
                OnPropertyChange(nameof(CurrentSupergroup));
            }
        }
        public GroupItems CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                OnPropertyChange(nameof(CurrentGroup));
            }
        }

        public ObservableCollection<Supergroup> Supergroups
        {
            get { return _supergroups; }
            set
            {
                _supergroups = value;
                OnPropertyChange(nameof(Supergroups));
            }
        }
        public ObservableCollection<GroupItems> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                OnPropertyChange(nameof(Groups));
            }
        }
        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInvoice
        {
            get { return _itemsInvoice; }
            set
            {
                _itemsInvoice = value;
                OnPropertyChange(nameof(ItemsInvoice));

                if(value != null && value.Any())
                {
                    HookOrderEnable = true;
                }
                else
                {
                    HookOrderEnable = false;
                }
            }
        }
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = Decimal.Round(value, 2);
                OnPropertyChange(nameof(TotalAmount));
            }
        }
        public int TableId
        {
            get { return _tableId; }
            set
            {
                _tableId = value;
                OnPropertyChange(nameof(TableId));
            }
        }
        
        public string Barkod
        {
            get { return _barkod; }
            set
            {
                if (!value.Contains('\n')) 
                {
                    _barkod = value;
                }

                OnPropertyChange(nameof(Barkod));

                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Contains('\n')) 
                    {
                        if (value[0] == '2')
                        {
                            string itemId = value.Substring(1, 6);
                            string quantityString = value.Substring(7, 5);

                            SqliteDbContext sqliteDbContext = new SqliteDbContext();

                            var itemDB = sqliteDbContext.Items.Find(itemId);

                            if(itemDB == null)
                            {
                                Log.Error($"SaleViewModel -> Barkod Property -> Greska u BARKODU {_barkod}: Ne postoji artikal sa šifrom iz barkod-a");
                                MessageBox.Show("Ne postoji artikal sa šifrom iz barkod-a",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            decimal quantity = -1;

                            try
                            {
                                quantity = Convert.ToDecimal(quantityString) / 1000;
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"SaleViewModel -> Barkod Property -> Greska u BARKODU {_barkod}: ", ex);
                                MessageBox.Show("Greška u količini u barkod-u",
                                    "Greška u količini",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            if(quantity <= 0)
                            {
                                Log.Error($"SaleViewModel -> Barkod Property -> Greska u količini u BARKODU {_barkod}: ");
                                MessageBox.Show("Greška u količini u barkod-u", 
                                    "Greška u količini",
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                                return;
                            }

                            if (itemDB != null)
                            {
                                Item item = new Item(itemDB);

                                var itemInInvoice = ItemsInvoice.FirstOrDefault(i => i.Item.Id == item.Id);

                                if (itemInInvoice != null)
                                {
                                    itemInInvoice.Quantity += quantity;

                                    decimal totalAmount = quantity * itemInInvoice.Item.SellingUnitPrice;

                                    itemInInvoice.TotalAmout += totalAmount;
                                    TotalAmount += totalAmount;
                                }
                                else
                                {
                                    ItemInvoice itemInvoice = new ItemInvoice(item, quantity);
                                    ItemsInvoice.Add(itemInvoice);
                                    TotalAmount += itemInvoice.TotalAmout;
                                }
                            }

                        }
                        else
                        {
                            decimal quantity = 0;

                            try
                            {
                                quantity = Decimal.Round(Convert.ToDecimal(Quantity), 3);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"SaleViewModel -> Barkod Property -> Greska u količini: ", ex);
                                MessageBox.Show("Greška u količini",
                                    "Greška u količini",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            if(quantity == 0)
                            {
                                Log.Error($"SaleViewModel -> Barkod Property -> kolicina ne sme da bude 0!");
                                MessageBox.Show("Količina ne sme da bude 0",
                                    "Greška u količini",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            SqliteDbContext sqliteDbContext = new SqliteDbContext();

                            var itemDB = sqliteDbContext.Items.FirstOrDefault(i => i.Barcode == _barkod);

                            if(itemDB != null)
                            {
                                Item item = new Item(itemDB);

                                var itemInInvoice = ItemsInvoice.FirstOrDefault(i => i.Item.Id == item.Id);

                                if (itemInInvoice != null)
                                {
                                    if(quantity >= itemInInvoice.Quantity)
                                    {
                                        TotalAmount -= itemInInvoice.TotalAmout;
                                        ItemsInvoice.Remove(itemInInvoice);
                                    }
                                    else
                                    {
                                        itemInInvoice.Quantity += quantity;
                                        decimal totalAmount = quantity * itemInInvoice.Item.SellingUnitPrice;

                                        itemInInvoice.TotalAmout += totalAmount;
                                        TotalAmount += totalAmount;
                                    }
                                }
                                else
                                {
                                    if (quantity > 0)
                                    {
                                        ItemInvoice itemInvoice = new ItemInvoice(item, quantity);
                                        ItemsInvoice.Add(itemInvoice);

                                        TotalAmount += itemInvoice.TotalAmout;
                                    }
                                    else
                                    {
                                        Log.Error($"SaleViewModel -> Barkod Property -> kolicina mora biti veca od 0!");
                                        MessageBox.Show("Količina da bude veća od 0",
                                            "Greška u količini",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                                        return;
                                    }
                                }
                            }
                        }

                        Quantity = "1";
                        Barkod = string.Empty;
                    }
                }
            }
        }
        public string Sifra
        {
            get { return _sifra; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        int sifraInt = Convert.ToInt32(value);
                        string sifra = sifraInt.ToString("000000");

                        value = sifra;

                        if(CurrentItem == null ||
                            CurrentItem.Id != sifra)
                        {
                            CurrentItem = ItemsSearch.FirstOrDefault(i => i.Id == sifra);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Neispravan format šifre.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    value = string.Empty;
                }

                _sifra = value;
                OnPropertyChange(nameof(Sifra));
            }
        }
        public string Quantity
        {
            get { return _quantity; }
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
                        var quantity = Decimal.Round(Convert.ToDecimal(value), 3);
                        value = quantity.ToString();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"SaleViewModel -> Quantity Property -> kolicina mora biti broj!", ex);
                        MessageBox.Show("Količina mora biti broj",
                            "Greška u količini",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        value = "1";
                    }
                }

                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
        public string Popust
        {
            get { return _popust; }
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
                        var popust = Decimal.Round(Convert.ToDecimal(value), 2);
                        value = popust.ToString();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"SaleViewModel -> Popust Property -> popust mora biti broj!", ex);
                        MessageBox.Show("Popust mora biti broj",
                            "Greška u popustu",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        value = "0";
                    }
                }
                else
                {
                    value = "0";
                }

                _popust = value;
                OnPropertyChange(nameof(Popust));
            }
        }
        public bool FirstChangeQuantity
        {
            get { return _firstChangeQuantity; }
            set
            {
                _firstChangeQuantity = value;
                OnPropertyChange(nameof(FirstChangeQuantity));
                if (value)
                {
                    Quantity = "1";
                }
            }
        }
        #endregion Properties

        #region Commands
        public ICommand ClickOnQuantityButtonCommand => new ClickOnQuantityButtonCommand(this);
        public ICommand UpdateAppViewModelCommand { get; set; }
        public ICommand LogoutCommand => new LogoutCommand(this);
        public ICommand SelectSupergroupCommand => new SelectSupergroupCommand(this);
        public ICommand SelectGroupCommand => new SelectGroupCommand(this);
        public ICommand SelectItemCommand => new SelectItemCommand(this);
        public ICommand ResetAllCommand => new ResetAllCommand(this);
        public ICommand PayCommand => new PayCommand(this);
        public ICommand HookOrderOnTableCommand => new HookOrderOnTableCommand(this);
        public ICommand TableOverviewCommand => new TableOverviewCommand(this);
        public ICommand ReduceQuantityCommand => new ReduceQuantityCommand(this);
        public ICommand PrintReportCommand => new PrintReportCommand(this);
        public ICommand RemoveOrderCommand => new RemoveOrderCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Internal methods
        internal void SetOrder(Order order)
        {
            TableId = order.TableId;
            CashierNema = order.Cashier.Name;
            TotalAmount = 0;
            ItemsInvoice = order.Items;
            order.Items.ToList().ForEach(item =>
            {
                TotalAmount += item.TotalAmout;
            });
        }
        internal void Reset()
        {
            CashierNema = LoggedCashier.Name;
            TableId = 0;
            TotalAmount = 0;
            ItemsInvoice = new ObservableCollection<ItemInvoice>();
            HookOrderEnable = false;
        }

        internal void SendToDisplay(string nameItem, string? priceItem = null)
        {
            try
            {
                if (_serialPort != null)
                {
                    string totalMesage = string.Empty;

                    if (string.IsNullOrEmpty(priceItem))
                    {
                        totalMesage += CenterString(nameItem, 20);
                        totalMesage += CenterString("PRIJATAN DAN!", 20);
                    }
                    else
                    {
                        totalMesage += SplitMessageToParts(priceItem, $"{nameItem}:", 20);
                        totalMesage += SplitMessageToParts(string.Format("{0:#,##0.00}", TotalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.'), "Ukupno:", 20);
                    }

                    _serialPort.Open();
                    _serialPort.Write(totalMesage);
                    _serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                UniversalEsir_Logging.Log.Error($"SaleViewModel - SendToDisplay - Greska prilikom slanja na COM port -> ", ex);
            }
        }

        internal async void FindItemInDB()
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            if (string.IsNullOrEmpty(Sifra))
            {
                MessageBox.Show("Niste uneli šifru!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Sifra = string.Empty;
                Quantity = "1";

                return;
            }

            var itemDB = await sqliteDbContext.Items.FindAsync(Sifra);

            if(itemDB == null)
            {
                MessageBox.Show("Ne postoji artikal sa zadatom šifrom!", "Greška", MessageBoxButton.OK,MessageBoxImage.Error);
                Sifra = string.Empty;
                Popust = "0";
                Quantity = "1";

                return;
            }

            if (string.IsNullOrEmpty(Quantity))
            {
                MessageBox.Show("Ne ispravna količina!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Sifra = string.Empty;
                Popust = "0";
                Quantity = "1";

                return;
            }

            decimal quantity = Convert.ToDecimal(Quantity);

            if (string.IsNullOrEmpty(Popust))
            {
                MessageBox.Show("Popust mora biti broj!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Sifra = string.Empty;
                Popust = "0";
                Quantity = "1";

                return;
            }

            decimal popust = Convert.ToDecimal(Popust);

            Item item = new Item(itemDB);

            if(popust > 0)
            {
                item.SellingUnitPrice = item.SellingUnitPrice * (1 - (popust / 100));
            }

            var itemInInvoice = ItemsInvoice.FirstOrDefault(i => i.Item.Id == item.Id);

            if (itemInInvoice != null)
            {
                itemInInvoice.Quantity += quantity;

                decimal totalAmount = quantity * item.SellingUnitPrice;

                itemInInvoice.TotalAmout += totalAmount;

                itemInInvoice.Item.SellingUnitPrice = Decimal.Round(itemInInvoice.TotalAmout / itemInInvoice.Quantity, 2);

                TotalAmount += totalAmount;
            }
            else
            {
                ItemInvoice itemInvoice = new ItemInvoice(item, quantity);
                ItemsInvoice.Add(itemInvoice);
                TotalAmount += itemInvoice.TotalAmout;
            }

            Sifra = string.Empty;
            Popust = "0";
            Quantity = "1";
            SearchNameText = string.Empty;
        }
        #endregion Internal methods

        #region Private methods
        private void RunTimer()
        {
            _timer = new Timer(
                async (e) => 
                {
                    CurrentDateTime = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
                },
                null,
                0,
                1000);
        }
        private static string SplitMessageToParts(string value, string fixedPart, int length)
        {
            string totalValue = value + fixedPart;

            int totalLength = totalValue.Length - length;
            if (totalLength > 0)
            {
                fixedPart = fixedPart.Substring(0, fixedPart.Length - totalLength - 2) + fixedPart[fixedPart.Length - 1];
            }

            string journal = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            journal = string.Format("{0}{1}", fixedPart, value.PadLeft(length - fixedPart.Length));

            return journal;
        }
        private static string CenterString(string value, int length)
        {
            string journal = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            int spaces = length - value.Length;
            int padLeft = spaces / 2 + value.Length;

            return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
        }
        #endregion Private methods
    }
}
