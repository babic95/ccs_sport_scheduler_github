using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.Sale
{
    public class ReduceQuantityCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _currentViewModel;

        public ReduceQuantityCommand(SaleViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string id = parameter as string;

            var items = _currentViewModel.ItemsInvoice.Where(item => item.Item.Id == id).FirstOrDefault();

            if (items is not null)
            {
                decimal quantity = 1;

                try
                {
                    quantity = Convert.ToDecimal(_currentViewModel.Quantity);
                    quantity = Decimal.Round(quantity, 3);
                }
                catch
                {
                    MessageBox.Show("Unesite ispravnu količinu!", "Ne ispravna količina", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (items.Quantity > quantity)
                {
                    items.Quantity -= quantity;

                    decimal totalAmount = items.Item.SellingUnitPrice * quantity;

                    items.TotalAmout -= totalAmount;
                    _currentViewModel.TotalAmount -= totalAmount;
                }
                else
                {
                    _currentViewModel.TotalAmount -= items.TotalAmout;
                    _currentViewModel.ItemsInvoice.Remove(items);
                }

                _currentViewModel.FirstChangeQuantity = true;
            }

            if (_currentViewModel.ItemsInvoice.Any())
            {
                _currentViewModel.HookOrderEnable = true;
            }
            else
            {
                _currentViewModel.HookOrderEnable = false;
            }
        }
    }
}
