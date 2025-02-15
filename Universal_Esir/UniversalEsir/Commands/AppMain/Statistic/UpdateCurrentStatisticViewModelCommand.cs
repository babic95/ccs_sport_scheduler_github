using UniversalEsir.Enums.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain;
using UniversalEsir.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic
{
    public class UpdateCurrentStatisticViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private StatisticsViewModel _currentViewModel;

        public UpdateCurrentStatisticViewModelCommand(StatisticsViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is StatisticStateEnumerable)
            {
                StatisticStateEnumerable viewType = (StatisticStateEnumerable)parameter;
                switch (viewType)
                {
                    case StatisticStateEnumerable.InventoryStatus:
                        if (_currentViewModel.CurrentViewModel is not InventoryStatusViewModel)
                        {
                            _currentViewModel.CheckedInventoryStatus();
                        }
                        break;
                    case StatisticStateEnumerable.AddEditSupplier:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!", 
                            "Nemate pravo korišćenja", 
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not AddEditSupplierViewModel)
                        {
                            _currentViewModel.CheckedAddEditSupplier();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.Calculation:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not CalculationViewModel)
                        {
                            _currentViewModel.CheckedCalculation();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.ViewCalculation:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not ViewCalculationViewModel)
                        {
                            _currentViewModel.CheckedViewCalculation();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.Nivelacija:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not NivelacijaViewModel)
                        {
                            _currentViewModel.CheckedNivelacija();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.ViewNivelacija:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not ViewNivelacijaViewModel)
                        {
                            _currentViewModel.CheckedViewNivelacija();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.Knjizenje:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not KnjizenjeViewModel)
                        {
                            _currentViewModel.CheckedKnjizenje();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.KEP:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not KEPViewModel)
                        {
                            _currentViewModel.CheckedKEP();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.PregledProknjizenogPazara:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not PregledPazaraViewModel)
                        {
                            _currentViewModel.CheckedViewKnjizenje();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.Refaund:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not RefaundViewModel)
                        {
                            _currentViewModel.CheckedRefaund();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.Norm:
#if CRNO
                        MessageBox.Show("Nemate pravo na korišćenja ove komande!",
                            "Nemate pravo korišćenja",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
#else
                        if (_currentViewModel.CurrentViewModel is not NormViewModel)
                        {
                            _currentViewModel.CheckedNorm();
                        }
#endif
                        break;
                    case StatisticStateEnumerable.PriceIncrease:
                        if (_currentViewModel.CurrentViewModel is not PriceIncreaseViewModel)
                        {
                            _currentViewModel.CheckedPriceIncrease();
                        }
                        break;
                    case StatisticStateEnumerable.Firma:
                        if (_currentViewModel.CurrentViewModel is not FirmaViewModel)
                        {
                            _currentViewModel.CheckedFirma();
                        }
                        break;
                    case StatisticStateEnumerable.Partner:
                        if (_currentViewModel.CurrentViewModel is not PartnerViewModel)
                        {
                            _currentViewModel.CheckedPartner();
                        }
                        break;
                    case StatisticStateEnumerable.Driver:
                        if (_currentViewModel.CurrentViewModel is not DriverViewModel)
                        {
                            _currentViewModel.CheckedDriver();
                        }
                        break;
                    case StatisticStateEnumerable.Otpremnice:
                        if (_currentViewModel.CurrentViewModel is not OtpremniceViewModel)
                        {
                            _currentViewModel.CheckedOtpremnice();
                        }
                        break;
                    case StatisticStateEnumerable.Clanovi:
                        if (_currentViewModel.CurrentViewModel is not ClanoviViewModel)
                        {
                            _currentViewModel.CheckedClanovi();
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
