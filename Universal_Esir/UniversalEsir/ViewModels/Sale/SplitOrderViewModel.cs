using UniversalEsir.Commands.Sale;
using UniversalEsir.Commands.Sale.Pay;
using UniversalEsir.Commands.Sale.Pay.SplitOrder;
using UniversalEsir.Models.Sale;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.ViewModels.Sale
{
    public class SplitOrderViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<ItemInvoice> _itemsInvoice;
        private ObservableCollection<ItemInvoice> _itemsInvoiceForPay;
        private ItemInvoice _selectedItemInvoice;
        private ItemInvoice _selectedItemInvoiceForPay;
        private decimal _totalAmount;
        private decimal _totalAmountForPay;
        private string _quantity;
        private bool _firstChangeQuantity;
        #endregion Fields

        #region Constructors
        public SplitOrderViewModel(Window window, PaySaleViewModel paySaleViewModel)
        {
            Window = window;
            PaySaleViewModel = paySaleViewModel;
            ItemsInvoice = paySaleViewModel.ItemsInvoice;
            TotalAmount = paySaleViewModel.TotalAmount;
            ItemsInvoiceForPay = new ObservableCollection<ItemInvoice>();

            Quantity = "1";
        }
        #endregion Constructors

        #region Properties
        public ObservableCollection<ItemInvoice> ItemsInvoice
        {
            get { return _itemsInvoice; }
            set
            {
                _itemsInvoice = value;
                OnPropertyChange(nameof(ItemsInvoice));
            }
        }
        public ObservableCollection<ItemInvoice> ItemsInvoiceForPay
        {
            get { return _itemsInvoiceForPay; }
            set
            {
                _itemsInvoiceForPay = value;
                OnPropertyChange(nameof(ItemsInvoiceForPay));
            }
        }

        public ItemInvoice SelectedItemInvoice
        {
            get { return _selectedItemInvoice; }
            set
            {
                _selectedItemInvoice = value;
                OnPropertyChange(nameof(SelectedItemInvoice));
            }
        }
        public ItemInvoice SelectedItemInvoiceForPay
        {
            get { return _selectedItemInvoiceForPay; }
            set
            {
                _selectedItemInvoiceForPay = value;
                OnPropertyChange(nameof(SelectedItemInvoiceForPay));
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
        public decimal TotalAmountForPay
        {
            get { return _totalAmountForPay; }
            set
            {
                _totalAmountForPay = value;
                OnPropertyChange(nameof(TotalAmountForPay));
            }
        }
        public string Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
        #endregion Properties

        #region Internal Properties
        internal Window Window { get; set; }
        internal PaySaleViewModel PaySaleViewModel { get; set; }
        #endregion Internal Properties

        #region Commands
        public ICommand CancelCommand => new CancelCommand(this);
        public ICommand PayCommand => new PayCommand(this);
        public ICommand MoveToOrderCommand => new MoveToOrderCommand(this);
        public ICommand MoveToPaymentCommand => new MoveToPaymentCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}