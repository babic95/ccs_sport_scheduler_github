using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Racuni;

namespace UniversalEsir.Models.AppMain.Statistic.Clanovi
{
    public class RacunItem : ObservableObject
    {
        private string _racunId;
        private string _itemsId;
        private string _name;
        private decimal _quantity;
        private decimal _unitPrice;
        private decimal _totalAmount;

        public RacunItem() { }
        public RacunItem(RacunItemResponse racunItem)
        {
            RacunId = racunItem.RacunId;
            ItemsId = racunItem.ItemsId;
            Name = racunItem.Name;
            Quantity = racunItem.Quantity;
            UnitPrice = racunItem.UnitPrice;
            TotalAmount = racunItem.TotalAmount;
        }

        public string RacunId
        {
            get { return _racunId; }
            set
            {
                _racunId = value;
                OnPropertyChange(nameof(RacunId));
            }
        }
        public string ItemsId
        {
            get { return _itemsId; }
            set
            {
                _itemsId = value;
                OnPropertyChange(nameof(ItemsId));
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
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChange(nameof(Quantity));
            }
        }
        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set
            {
                _unitPrice = value;
                OnPropertyChange(nameof(UnitPrice));
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
    }
}
