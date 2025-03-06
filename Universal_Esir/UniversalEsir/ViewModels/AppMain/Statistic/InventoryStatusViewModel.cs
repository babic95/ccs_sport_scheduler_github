using UniversalEsir.Commands.AppMain.Statistic;
using UniversalEsir.Commands.AppMain.Statistic.Calculation;
using UniversalEsir.Commands.AppMain.Statistic.InventoryStatus;
using UniversalEsir.Commands.AppMain.Statistic.Norm;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.Sale;
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
    public class InventoryStatusViewModel : ViewModelBase
    {
        #region Fields
        private Supergroup? _currentSupergroup;
        private GroupItems? _currentGroupItems;

        private ObservableCollection<Supergroup> _allSupergroups;
        private ObservableCollection<GroupItems> _allGroupItems;

        private Invertory _currentInventoryStatus; 
        private ObservableCollection<Invertory> _inventoryStatus;
        private ObservableCollection<Invertory> _inventoryStatusNorm;
        private Invertory _currentInventoryStatusNorm;

        private ObservableCollection<Invertory> _norma;

        private string _searchText;
        private decimal _normQuantity;
        private string _normQuantityString;
        private string _searchItems;

        private bool _editItemIsReadOnly;
        private bool _isReadOnlyItemId;

        private Visibility _visibilityNext;
        private Visibility _visibilityAllSupergroup;
        private Visibility _visibilityAllGroupItems;

        private string _quantityCommandParameter;
        private ObservableCollection<GroupItems> _allGroups;
        private GroupItems _currentGroup;

        private ObservableCollection<TaxLabel> _allLabels;
        private TaxLabel _currentLabel;
#if DEBUG
        private List<TaxLabel> _labels = new List<TaxLabel>()
        {
            new TaxLabel("31", "A - 9% PDV"),
            new TaxLabel("47", "N - 0% PDV"),
            new TaxLabel("8", "Ж - 19% PDV"),
            new TaxLabel("39", "F - 11% PDV"),
        };
#else
        private List<TaxLabel> _labels = new List<TaxLabel>()
        {
            new TaxLabel("1", "A - Nije u PDV"),
            new TaxLabel("4", "Г - 0% PDV"),
            new TaxLabel("6", "Ђ - 20% PDV"),
            new TaxLabel("7", "Е - 10% PDV"),
        };
#endif
#endregion Fields

        #region Constructors
        public InventoryStatusViewModel()
        {
            AllGroupItems = new ObservableCollection<GroupItems>();
            AllSupergroups = new ObservableCollection<Supergroup>();
            AllGroups = new ObservableCollection<GroupItems>() { new GroupItems(-1 , -1, "Sve grupe") };

            AllLabels = new ObservableCollection<TaxLabel>(_labels);

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            if (sqliteDbContext.Items != null &&
                sqliteDbContext.Items.Any())
            {
                sqliteDbContext.Items.ToList().ForEach(x =>
                {
                    Item item = new Item(x);
                    var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);

                    if (group != null)
                    {
                        bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                        InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity == null ? -1 : x.AlarmQuantity.Value, isSirovina));
                    }
                });
            }

            if (sqliteDbContext.Supergroups != null &&
                sqliteDbContext.Supergroups.Any())
            {
                sqliteDbContext.Supergroups.ToList().ForEach(supergroup =>
                {
                    AllSupergroups.Add(new Supergroup(supergroup.Id, supergroup.Name));
                });

                CurrentSupergroup = AllSupergroups.FirstOrDefault();
            }

            if (sqliteDbContext.ItemGroups != null &&
                sqliteDbContext.ItemGroups.Any())
            {
                sqliteDbContext.ItemGroups.ToList().ForEach(gropu =>
                {
                    AllGroupItems.Add(new GroupItems(gropu.Id, gropu.IdSupergroup, gropu.Name));
                    AllGroups.Add(new GroupItems(gropu.Id, gropu.IdSupergroup, gropu.Name));
                });

                CurrentGroupItems = AllGroupItems.FirstOrDefault();
                CurrentGroup = AllGroups.FirstOrDefault();
            }

            Norma = new ObservableCollection<Invertory>();

            InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
            InventoryStatusNorm = new ObservableCollection<Invertory>(InventoryStatusAll);

            NormQuantityString = "0";
            VisibilityNext = Visibility.Hidden;
        }
        #endregion Constructors

        #region Properties internal
        internal List<Invertory> InventoryStatusAll = new List<Invertory>();
        internal Window Window { get; set; }
        internal Window WindowHelper { get; set; }
        internal int CurrentNorm { get; set; }
        internal Window PrintTypeWindow { get; set; }
        internal bool ItemForEdit { get; set; }
        #endregion Properties internal

        #region Properties
        public ObservableCollection<Supergroup> AllSupergroups
        {
            get { return _allSupergroups; }
            set
            {
                _allSupergroups = value;
                OnPropertyChange(nameof(AllSupergroups));
            }
        }
        public ObservableCollection<GroupItems> AllGroupItems
        {
            get { return _allGroupItems; }
            set
            {
                _allGroupItems = value;
                OnPropertyChange(nameof(AllGroupItems));
            }
        }
        public ObservableCollection<GroupItems> AllGroups
        {
            get { return _allGroups; }
            set
            {
                _allGroups = value;
                OnPropertyChange(nameof(AllGroups));
            }
        }
        
        public ObservableCollection<TaxLabel> AllLabels
        {
            get { return _allLabels; }
            set
            {
                _allLabels = value;
                OnPropertyChange(nameof(AllLabels));
            }
        }
        public TaxLabel CurrentLabel
        {
            get { return _currentLabel; }
            set
            {
                _currentLabel = value;
                OnPropertyChange(nameof(CurrentLabel));

                if (value != null &&
                    CurrentInventoryStatus != null &&
                    CurrentInventoryStatus.Item != null)
                {
                    CurrentInventoryStatus.Item.Label = value.Id;
                }
            }
        }
        public Supergroup? CurrentSupergroup
        {
            get { return _currentSupergroup; }
            set
            {
                _currentSupergroup = value;
                OnPropertyChange(nameof(CurrentSupergroup));
            }
        }
        public GroupItems? CurrentGroupItems
        {
            get { return _currentGroupItems; }
            set
            {
                _currentGroupItems = value;

                if (value != null)
                {
                    CurrentSupergroup = AllSupergroups.FirstOrDefault(supergroup => supergroup.Id == value.IdSupergroup);

                    if (CurrentInventoryStatus != null)
                    {
                        if (value.Name.ToLower().Contains("sirovina") ||
                            value.Name.ToLower().Contains("sirovine"))
                        {
                            CurrentInventoryStatus.VisibilityJC = Visibility.Hidden;
                        }
                        else
                        {
                            CurrentInventoryStatus.VisibilityJC = Visibility.Visible;
                        }
                    }
                }
                OnPropertyChange(nameof(CurrentGroupItems));
            }
        }
        public GroupItems CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                OnPropertyChange(nameof(CurrentGroup));

                if(value.Id == -1)
                {
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
                }
                else
                {
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll.Where(inventory => inventory.IdGroupItems == value.Id));
                }
            }
        }
        public Visibility VisibilityNext
        {
            get { return _visibilityNext; }
            set
            {
                _visibilityNext = value;
                OnPropertyChange(nameof(VisibilityNext));
            }
        }
        public Visibility VisibilityAllSupergroup
        {
            get { return _visibilityAllSupergroup; }
            set
            {
                _visibilityAllSupergroup = value;
                OnPropertyChange(nameof(VisibilityAllSupergroup));
            }
        }
        public bool IsReadOnlyItemId
        {
            get { return _isReadOnlyItemId; }
            set
            {
                _isReadOnlyItemId = value;
                OnPropertyChange(nameof(IsReadOnlyItemId));
            }
        }
        public Visibility VisibilityAllGroupItems
        {
            get { return _visibilityAllGroupItems; }
            set
            {
                _visibilityAllGroupItems = value;
                OnPropertyChange(nameof(VisibilityAllGroupItems));
            }
        }
        public decimal NormQuantity
        {
            get { return _normQuantity; }
            set
            {
                _normQuantity = value;
                OnPropertyChange(nameof(NormQuantity));
            }
        }
        public string NormQuantityString
        {
            get { return _normQuantityString; }
            set
            {
                _normQuantityString = value.Replace(',', '.');
                OnPropertyChange(nameof(NormQuantityString));

                try
                {
                    NormQuantity = Convert.ToDecimal(_normQuantityString);
                }
                catch
                {
                    NormQuantityString = "0";
                }
            }
        }

        public bool EditItemIsReadOnly
        {
            get { return _editItemIsReadOnly; }
            set
            {
                _editItemIsReadOnly = value;
                OnPropertyChange(nameof(EditItemIsReadOnly));
            }
        }
        public Invertory CurrentInventoryStatus
        {
            get { return _currentInventoryStatus; }
            set
            {
                _currentInventoryStatus = value;
                OnPropertyChange(nameof(CurrentInventoryStatus));

                if(value != null)
                {
                    VisibilityNext = Visibility.Visible;

                    if(value.Item != null &&
                        !string.IsNullOrEmpty(value.Item.Label))
                    {
                        var label = AllLabels.FirstOrDefault(lab => lab.Id == value.Item.Label);

                        if (label != null)
                        {
                            CurrentLabel = label;
                        }
                    }
                }
                else
                {
                    VisibilityNext = Visibility.Hidden;
                }
            }
        }
        public Invertory CurrentInventoryStatusNorm
        {
            get { return _currentInventoryStatusNorm; }
            set
            {
                _currentInventoryStatusNorm = value;
                OnPropertyChange(nameof(CurrentInventoryStatusNorm));
            }
        }
        public ObservableCollection<Invertory> Norma
        {
            get { return _norma; }
            set
            {
                _norma = value;
                OnPropertyChange(nameof(Norma));
            }
        }
        public ObservableCollection<Invertory> InventoryStatus
        {
            get { return _inventoryStatus; }
            set
            {
                _inventoryStatus = value;
                OnPropertyChange(nameof(InventoryStatus));
            }
        }
        public ObservableCollection<Invertory> InventoryStatusNorm
        {
            get { return _inventoryStatusNorm; }
            set
            {
                _inventoryStatusNorm = value;
                OnPropertyChange(nameof(InventoryStatusNorm));
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
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll);
                }
                else
                {
                    InventoryStatus = new ObservableCollection<Invertory>(InventoryStatusAll.Where(inventory => inventory.Item.Name.ToLower().Contains(value.ToLower())));
                }
            }
        }
        public string SearchItems
        {
            get { return _searchItems; }
            set
            {
                _searchItems = value;
                OnPropertyChange(nameof(SearchItems));

                if (string.IsNullOrEmpty(value))
                {
                    InventoryStatusNorm = new ObservableCollection<Invertory>(InventoryStatusAll);
                }
                else
                {
                    InventoryStatusNorm = new ObservableCollection<Invertory>(InventoryStatusAll.Where(inventory => inventory.Item.Name.ToLower().Contains(value.ToLower())));

                    if(CurrentInventoryStatusNorm != null &&
                        !InventoryStatusNorm.Where(inventory => inventory.Item.Id == CurrentInventoryStatusNorm.Item.Id).Any())
                    {
                        VisibilityNext = Visibility.Hidden;
                        CurrentInventoryStatusNorm = null;
                    }
                }
            }
        }

        public string QuantityCommandParameter
        {
            get { return _quantityCommandParameter; }
            set
            {
                _quantityCommandParameter = value;
                OnPropertyChange(nameof(QuantityCommandParameter));
            }
        }
#endregion Properties

        #region Commands
        public ICommand OpenAddEditWindow => new OpenAddEditWindow(this);
        public ICommand OpenPrintCommand => new OpenPrintCommand(this);
        public ICommand PrintCommand => new PrintCommand(this);
        public ICommand PrintA4Command => new PrintA4Command(this);
        public ICommand SaveCommand => new SaveCommand(this);
        public ICommand EditCommand => new EditCommand(this);
        public ICommand DeleteCommand => new DeleteCommand(this);
        public ICommand EditNormCommand => new EditNormCommand(this);
        public ICommand DeleteNormCommand => new DeleteNormCommand(this);
        public ICommand NextCommand => new NextCommand(this);
        public ICommand OpenNormativWindowCommand => new OpenNormativWindowCommand(this);
        public ICommand OpenAddOrEditSupergroupCommand => new OpenAddOrEditSupergroupCommand(this);
        public ICommand OpenAddOrEditGroupItemsCommand => new OpenAddOrEditGroupItemsCommand(this);
        public ICommand SaveSupergroupCommand => new SaveSupergroupCommand(this);
        public ICommand SaveGroupItemsCommand => new SaveGroupItemsCommand(this);
        public ICommand FixInputPriceCommand => new SaveCalculationCommand(this);
        public ICommand FixQuantityCommand => new FixQuantityCommand();
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}