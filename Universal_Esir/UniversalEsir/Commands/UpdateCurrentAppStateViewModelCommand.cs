using UniversalEsir.Enums;
using UniversalEsir.State.Navigators;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain;
using UniversalEsir.ViewModels.Login;
using UniversalEsir.ViewModels.Sale;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace UniversalEsir.Commands
{
    public class UpdateCurrentAppStateViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private INavigator _navigator;

        public UpdateCurrentAppStateViewModelCommand(INavigator navigator)
        {
            _navigator = navigator;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is AppStateParameter)
            {
                AppStateParameter appState = (AppStateParameter)parameter;
                switch (appState.State)
                {
                    case AppStateEnumerable.Login:
                        if (SettingsManager.Instance.EnableSmartCard())
                        {
                            _navigator.CurrentViewModel = new LoginCardViewModel(_navigator.UpdateCurrentViewModelCommand);
                        }
                        else
                        {
                            _navigator.CurrentViewModel = new LoginViewModel(_navigator.UpdateCurrentViewModelCommand);
                        }
                        break;
                    case AppStateEnumerable.Main:
                        _navigator.CurrentViewModel = new AppMainViewModel(appState.LoggedCashier, _navigator.UpdateCurrentViewModelCommand);
                        break;
                    case AppStateEnumerable.Sale:
                        if (appState.ViewModel is SaleViewModel saleViewModel)
                        {
                            if (!(_navigator.CurrentViewModel is SaleViewModel))
                            {
                                _navigator.CurrentViewModel = saleViewModel;
                            }
                        }
                        else
                        {
                            if (!(_navigator.CurrentViewModel is SaleViewModel))
                            {
                                var newSaleViewModel = (SaleViewModel)appState.ViewModel;
                                // Set additional properties if needed
                                _navigator.CurrentViewModel = newSaleViewModel;
                            }
                        }
                        break;
                    case AppStateEnumerable.TableOverview:
                        if (appState.ViewModel is SaleViewModel sale)
                        {
                            if (!(_navigator.CurrentViewModel is TableOverviewViewModel))
                            {
                                sale.TableOverviewViewModel = new TableOverviewViewModel(sale);

                                _navigator.CurrentViewModel = sale.TableOverviewViewModel;
                            }
                        }
                        else
                        {
                            if (!(_navigator.CurrentViewModel is TableOverviewViewModel))
                            {
                                var saleVM = new SaleViewModel(appState.LoggedCashier, _navigator.UpdateCurrentViewModelCommand);

                                _navigator.CurrentViewModel = saleVM.TableOverviewViewModel;
                            }
                        }
                        break;
                    default:
                        break;
                }
                return;
            }
        }
    }
}
