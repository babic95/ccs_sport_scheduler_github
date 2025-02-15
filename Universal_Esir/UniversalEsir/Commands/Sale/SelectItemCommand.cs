using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.Sale
{
    public class SelectItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public SelectItemCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            string idItem = (string)parameter;

            decimal quantity = 1;
            try
            {
                quantity = Convert.ToDecimal(_viewModel.Quantity);
                quantity = Math.Round(quantity, 3);
            }
            catch
            {
                MessageBox.Show("Unesite ispravnu količinu!", "Ne ispravna količina", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ItemDB? itemDB = _viewModel.AllItems.Where(item => item.Id == idItem).FirstOrDefault();

            if(itemDB is null)
            {
                return;
            }

            ItemInvoice? itemInvoice = _viewModel.ItemsInvoice.Where(item => item.Item.Id == itemDB.Id).FirstOrDefault();

            if (itemInvoice is not null)
            {
                itemInvoice.Quantity += quantity;

                itemInvoice.TotalAmout += itemDB.SellingUnitPrice * quantity;

                _viewModel.TotalAmount += itemDB.SellingUnitPrice * quantity;
            }
            else
            {
                _viewModel.ItemsInvoice.Add(new ItemInvoice(new Item(itemDB), quantity));

                _viewModel.TotalAmount += itemDB.SellingUnitPrice * quantity;
            }

            _viewModel.SendToDisplay(itemDB.Name, string.Format("{0:#,##0.00}", itemDB.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.'));

            if (_viewModel.ItemsInvoice.Any())
            {
                _viewModel.HookOrderEnable = true;
            }
            else
            {
                _viewModel.HookOrderEnable = false;
            }

            _viewModel.FirstChangeQuantity = true;
        }
    }
}
