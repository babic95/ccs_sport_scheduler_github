using UniversalEsir_Database.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir.Models.Sale
{
    public class Item : ObservableObject
    {
        private string _id;
        private string? _barcode;
        private string _name;
        private decimal _sellingUnitPrice;
        private decimal? _inputUnitPrice;
        private decimal _originalUnitPrice;
        private decimal _quantity;
        private string _label;
        private string _jm;
        private bool _isKonobar;

        public Item() { }
        public Item(ItemDB itemDB)
        {
            Id = itemDB.Id;
            Barcode = itemDB.Barcode;
            Name = itemDB.Name;
            InputUnitPrice = itemDB.InputUnitPrice;
            SellingUnitPrice = itemDB.SellingUnitPrice;
            OriginalUnitPrice = itemDB.SellingUnitPrice;
            Label = itemDB.Label;
            Jm = itemDB.Jm;
            Quantity = itemDB.TotalQuantity;
            IsKonobar = itemDB.IsKonobarItem == 1 ? true : false;
        }
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public string? Barcode
        {
            get { return _barcode; }
            set
            {
                _barcode = value;
                OnPropertyChange(nameof(Barcode));
            }
        }
        
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
            }
        }
        public string Jm
        {
            get { return _jm; }
            set
            {
                _jm = value;
                OnPropertyChange(nameof(Jm));
            }
        }
        public decimal SellingUnitPrice
        {
            get { return _sellingUnitPrice; }
            set
            {
                _sellingUnitPrice = Decimal.Round(value, 2);
                OnPropertyChange(nameof(SellingUnitPrice));
            }
        }
        public decimal? InputUnitPrice
        {
            get { return _inputUnitPrice; }
            set
            {
                if (value != null &&
                    value.HasValue)
                {
                    _inputUnitPrice = Decimal.Round(value.Value, 2);
                }
                else
                {
                    _inputUnitPrice = null;
                }
                OnPropertyChange(nameof(InputUnitPrice));
            }
        }
        public decimal OriginalUnitPrice
        {
            get { return _originalUnitPrice; }
            set
            {
                _originalUnitPrice = Decimal.Round(value, 2);
                OnPropertyChange(nameof(OriginalUnitPrice));
            }
        }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = Decimal.Round(value, 2);
                OnPropertyChange(nameof(Quantity));
            }
        }
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChange(nameof(Label));
            }
        }
        public bool IsKonobar
        {
            get { return _isKonobar; }
            set
            {
                _isKonobar = value;
                OnPropertyChange(nameof(IsKonobar));
            }
        }
    }
}
