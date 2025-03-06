using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir.Models.Sale
{
    public class Order : ObservableObject
    {
        private int _partHall;
        private int _tableId;
        private string _username;
        private CashierDB _cashier;
        private string _cashierName;
        private ObservableCollection<ItemInvoice> _items;

        public Order(int tableId, int partHall, string username)
        {
            TableId = tableId;
            PartHall = partHall;
            Username = username;
        }
        public Order(CashierDB cashier,
            ObservableCollection<ItemInvoice> items)
        {
            Cashier = cashier;
            Items = items;
        }

        public int PartHall
        {
            get { return _partHall; }
            set
            {
                _partHall = value;
                OnPropertyChange(nameof(PartHall));
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
        public CashierDB Cashier
        {
            get { return _cashier; }
            set
            {
                _cashier = value;
                CashierName = value.Name;
                OnPropertyChange(nameof(Cashier));
            }
        }
        public string CashierName
        {
            get { return _cashierName; }
            set
            {
                _cashierName = value;
                OnPropertyChange(nameof(CashierName));
            }
        }
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChange(nameof(Username));
            }
        }
        public ObservableCollection<ItemInvoice> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChange(nameof(Items));
            }
        }
    }
}
