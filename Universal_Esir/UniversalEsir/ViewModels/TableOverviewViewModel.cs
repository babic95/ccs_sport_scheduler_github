using UniversalEsir.Commands.AppMain.Admin;
using UniversalEsir.Commands.TableOverview;
using UniversalEsir.Enums.AppMain.Admin;
using UniversalEsir.Models.Sale;
using UniversalEsir.Models.TableOverview;
using UniversalEsir_Common.Enums;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Printer.PaperFormat;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniversalEsir.ViewModels
{
    public class TableOverviewViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<PartHall> _rooms;
        private ObservableCollection<PaymentPlace> _allNormalPaymentPlaces;
        private ObservableCollection<PaymentPlace> _normalPaymentPlaces;

        private ObservableCollection<PaymentPlace> _allRoundPaymentPlaces;
        private ObservableCollection<PaymentPlace> _roundPaymentPlaces;
        private PartHall? _currentPartHall;
        private string _title;
        #endregion Fields

        #region Constructors
        public TableOverviewViewModel(SaleViewModel saleViewModel)
        {
            SaleViewModel = saleViewModel;
            Order = saleViewModel.CurrentOrder;

            Rooms = new ObservableCollection<PartHall>();
            AllNormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            NormalPaymentPlaces = new ObservableCollection<PaymentPlace>();
            AllRoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            RoundPaymentPlaces = new ObservableCollection<PaymentPlace>();
            LoadingDB();

            if (SaleViewModel.CurrentPartHall == null)
            {
                CurrentPartHall = Rooms.FirstOrDefault();
            }
            else
            {
                CurrentPartHall = Rooms.FirstOrDefault(r => r.Id == SaleViewModel.CurrentPartHall.Id);
            }
        }
        #endregion Constructors

        #region Properties
        public ObservableCollection<PartHall> Rooms
        {
            get { return _rooms; }
            set
            {
                _rooms = value;
                OnPropertyChange(nameof(Rooms));
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
        public ObservableCollection<PaymentPlace> NormalPaymentPlaces
        {
            get { return _normalPaymentPlaces; }
            set
            {
                _normalPaymentPlaces = value;
                OnPropertyChange(nameof(NormalPaymentPlaces));
            }
        }

        public ObservableCollection<PaymentPlace> AllNormalPaymentPlaces
        {
            get { return _allNormalPaymentPlaces; }
            set
            {
                _allNormalPaymentPlaces = value;
                OnPropertyChange(nameof(AllNormalPaymentPlaces));
            }
        }

        public ObservableCollection<PaymentPlace> RoundPaymentPlaces
        {
            get { return _roundPaymentPlaces; }
            set
            {
                _roundPaymentPlaces = value;
                OnPropertyChange(nameof(RoundPaymentPlaces));
            }
        }

        public ObservableCollection<PaymentPlace> AllRoundPaymentPlaces
        {
            get { return _allRoundPaymentPlaces; }
            set
            {
                _allRoundPaymentPlaces = value;
                OnPropertyChange(nameof(AllRoundPaymentPlaces));
            }
        }
        public PartHall? CurrentPartHall
        {
            get { return _currentPartHall; }
            set
            {
                if (_currentPartHall != value)
                {
                    _currentPartHall = value;
                    OnPropertyChange(nameof(CurrentPartHall));

                    if (value != null)
                    {
                        NormalPaymentPlaces = new ObservableCollection<PaymentPlace>(AllNormalPaymentPlaces.Where(p => p.PartHallId == value.Id));
                        RoundPaymentPlaces = new ObservableCollection<PaymentPlace>(AllRoundPaymentPlaces.Where(p => p.PartHallId == value.Id));
                    }
                }
            }
        }
        #endregion Properties

        #region Internal Properties
        internal SaleViewModel SaleViewModel { get; set; }
        internal Order? Order { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand SelectRoomCommand => new SelectRoomCommand(this); 
        public ICommand CancelCommand => new CancelCommand(this);
        public ICommand ClickOnPaymentPlaceCommand => new ClickOnPaymentPlaceCommand(this);
        #endregion Commands

        #region Public methods
        public void SetOrder(Order order)
        {
            PaymentPlace? paymentPlace = NormalPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);

            if (paymentPlace == null)
            {
                paymentPlace = RoundPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);
            }

            if (paymentPlace != null)
            {
                if (paymentPlace.Order != null && paymentPlace.Order.Items.Any())
                {
                    order.Items.ToList().ForEach(item =>
                    {
                        var itemInPaymentPlace = paymentPlace.Order.Items.FirstOrDefault(i => i.Item.Id == item.Item.Id);
                        if(itemInPaymentPlace != null)
                        {
                            itemInPaymentPlace.TotalAmout += item.TotalAmout;
                            itemInPaymentPlace.Quantity += item.Quantity;
                        }
                        else
                        {
                            paymentPlace.Order.Items.Add(item);
                        }
                    });
                }
                else
                {
                    paymentPlace.Order = new Order(order.TableId, order.PartHall, order.Username)
                    {
                        Items = new ObservableCollection<ItemInvoice>(order.Items),
                        Cashier = order.Cashier
                    };
                    paymentPlace.Total = SaleViewModel.TotalAmount;
                    paymentPlace.Background = Brushes.Red;
                }

                SaleViewModel.TableId = 0;
                SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>();
                SaleViewModel.TotalAmount = 0;
            }
        }
        #endregion Public methods

        #region Private methods
        private void LoadingDB()
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            sqliteDbContext.PartHalls.ToList().ForEach(part =>
            {
                PartHall partHall = new PartHall()
                {
                    Id = part.Id,
                    Name = part.Name,
                    Image = part.Image
                };

                Rooms.Add(partHall);
            });

            sqliteDbContext.PaymentPlaces.ToList().ForEach(payment =>
            {
                PaymentPlace paymentPlace = new PaymentPlace()
                {
                    Id = payment.Id,
                    PartHallId = payment.PartHallId,
                    Left = payment.LeftCanvas.Value,
                    Top = payment.TopCanvas.Value,
                    Type = payment.Type.HasValue ? (PaymentPlaceTypeEnumeration)payment.Type.Value : PaymentPlaceTypeEnumeration.Normal,
                    Name = payment.Name,
                    AddPrice = payment.AddPrice,
                    UserId = payment.UserId,
                };

                if (paymentPlace.Type == PaymentPlaceTypeEnumeration.Normal)
                {
                    paymentPlace.Width = payment.Width.Value;
                    paymentPlace.Height = payment.Height.Value;
                }
                else
                {
                    paymentPlace.Diameter = payment.Width.Value;
                }

                var unprocessedOrders = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == payment.Id);

                if (unprocessedOrders != null)
                {
                    CashierDB? cashierDB = sqliteDbContext.Cashiers.Find(unprocessedOrders.CashierId);
                    var itemsInUnprocessedOrder = sqliteDbContext.Items.Join(sqliteDbContext.ItemsInUnprocessedOrder,
                        item => item.Id,
                        itemInUnprocessedOrder => itemInUnprocessedOrder.ItemId,
                        (item, itemInUnprocessedOrder) => new { Item = item, ItemInUnprocessedOrder = itemInUnprocessedOrder })
                    .Where(item => item.ItemInUnprocessedOrder.UnprocessedOrderId == unprocessedOrders.Id);

                    if (cashierDB != null && itemsInUnprocessedOrder.Any())
                    {
                        ObservableCollection<ItemInvoice> items = new ObservableCollection<ItemInvoice>();
                        decimal total = 0;

                        itemsInUnprocessedOrder.ToList().ForEach(item =>
                        {
                            ItemInvoice itemInvoice = new ItemInvoice(new Item(item.Item), item.ItemInUnprocessedOrder.Quantity);
                            items.Add(itemInvoice);
                            total += itemInvoice.TotalAmout;
                        });

                        paymentPlace.Order = new Order(cashierDB, items);
                        paymentPlace.Order.TableId = payment.Id;
                        paymentPlace.Order.PartHall = payment.PartHallId;
                        paymentPlace.Background = Brushes.Red;
                        paymentPlace.Total = total;
                    }
                }
                else
                {
                    paymentPlace.Order = new Order(payment.Id, payment.PartHallId, payment.Name);
                    paymentPlace.Background = Brushes.Green;
                    paymentPlace.Total = 0;
                }

                switch (paymentPlace.Type)
                {
                    case PaymentPlaceTypeEnumeration.Normal:
                        AllNormalPaymentPlaces.Add(paymentPlace);
                        break;
                    case PaymentPlaceTypeEnumeration.Round:
                        AllRoundPaymentPlaces.Add(paymentPlace);
                        break;
                }
            });
        }
        #endregion Private methods
    }
}
