using UniversalEsir.Enums;
using UniversalEsir.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.TableOverview
{
    public class CancelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TableOverviewViewModel _currentView;

        public CancelCommand(TableOverviewViewModel currentView)
        {
            _currentView = currentView;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.Sale, 
                _currentView.SaleViewModel.LoggedCashier,
                _currentView.SaleViewModel);
            _currentView.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
        }
    }
}
