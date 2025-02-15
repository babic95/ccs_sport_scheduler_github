using UniversalEsir.ViewModels.Sale;
using UniversalEsir.Views.Sale.PaySale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.Sale.Pay
{
    public class SplitOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PaySaleViewModel _viewModel;

        public SplitOrderCommand(PaySaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            _viewModel.SplitOrder = true;
            SplitOrderWindow splitOrderWindow = new SplitOrderWindow(_viewModel);
            splitOrderWindow.ShowDialog();
        }
    }
}