using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Common.Models.Statistic.Driver;
using UniversalEsir_Database;
using UniversalEsir_Logging;
using UniversalEsir_Printer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class PrintAllIsporukeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public PrintAllIsporukeCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                if (_currentViewModel.AllIsporuke.Any())
                {
                    List<IsporukaGlobal> isporuke = new List<IsporukaGlobal>();
                    _currentViewModel.AllIsporuke.ToList().ForEach(isporuka =>
                    {
                        IsporukaGlobal isporukaGlobal = new IsporukaGlobal()
                        {
                            Counter = isporuka.Counter.ToString(),
                            DateCreate = isporuka.CreateDate.ToString("dd.MM.yyyy"),
                            DateIsporuke = isporuka.DateIsporuka.HasValue ? isporuka.DateIsporuka.Value.ToString("dd.MM.yyyy") : string.Empty,
                            IsporukaName = $"Isporuka_{isporuka.Counter}",
                            TotalAmount = string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(isporuka.TotalAmount), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.')
                        };

                        isporuke.Add(isporukaGlobal);
                    });

                    DriverGlobal driverGlobal = new DriverGlobal()
                    {
                        Id = _currentViewModel.CurrentDriver.Id,
                        Name = _currentViewModel.CurrentDriver.Name,
                        Email = _currentViewModel.CurrentDriver.Email,
                        ContractNumber = _currentViewModel.CurrentDriver.ContractNumber,
                    };

                    PrinterManager.Instance.PrintAllIsporuke(isporuke,
                        driverGlobal,
                        _currentViewModel.StartDate.ToString("dd.MM.yyyy"),
                        _currentViewModel.EndDate.ToString("dd.MM.yyyy"),
                        string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(_currentViewModel.TotalAmountIsporukefromDriver), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.'));
                }
                else
                {
                    Log.Debug($"PrintAllIsporukeCommand -> Execute -> Vozac nema isporuka za zadat period!");
                    MessageBox.Show("Vozač nema isporuka za zadati period!",
                        "Vozač nema isporuka",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PrintAllIsporukeCommand -> Execute -> Greska prilikom stampe svih isporuka", ex);
                MessageBox.Show("Greška prilikom štampe svih isporuka.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}