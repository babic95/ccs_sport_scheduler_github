using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic
{
    public class OpenAddEditWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public OpenAddEditWindow(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is AddEditSupplierViewModel)
            {
                AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;

                AddEditSupplierWindow addEditSupplierWindow = new AddEditSupplierWindow(addEditSupplierViewModel);
                addEditSupplierWindow.ShowDialog();
            }
            else if (_currentViewModel is InventoryStatusViewModel)
            {
                InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

                inventoryStatusViewModel.ItemForEdit = false;
                inventoryStatusViewModel.IsReadOnlyItemId = false;
                inventoryStatusViewModel.CurrentGroupItems = inventoryStatusViewModel.AllGroupItems.FirstOrDefault();
                inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                inventoryStatusViewModel.CurrentInventoryStatus = new Invertory();
                inventoryStatusViewModel.CurrentNorm = -1;
                inventoryStatusViewModel.EditItemIsReadOnly = false;
                AddEditItemWindow addEditItemWindow = new AddEditItemWindow(inventoryStatusViewModel);
                addEditItemWindow.ShowDialog();
            }
        }
    }
}