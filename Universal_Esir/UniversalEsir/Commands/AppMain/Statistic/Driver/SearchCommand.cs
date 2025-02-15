using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class SearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public SearchCommand(DriverViewModel currentViewModel)
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
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                _currentViewModel.AllIsporuke = new ObservableCollection<Isporuka>();
                _currentViewModel.TotalAmountIsporukefromDriver = 0;

                var allIsporuke = sqliteDbContext.Isporuke.Join(sqliteDbContext.DriverInvoices,
                    isporuke => isporuke.Id,
                    driverInvoice => driverInvoice.IsporukaId,
                    (isporuke, driverInvoice) => new { Isporuka = isporuke, DriverInvoice = driverInvoice })
                    .Where(isporuke => isporuke.DriverInvoice.DriverId == _currentViewModel.CurrentDriver.Id &&
                    isporuke.DriverInvoice.IsporukaId != null &&
                    isporuke.Isporuka.DateIsporuka.Date >= _currentViewModel.StartDate.Date &&
                    isporuke.Isporuka.DateIsporuka.Date <= _currentViewModel.EndDate.Date).Select(isporuka => isporuka.Isporuka);

                if (allIsporuke != null &&
                    allIsporuke.Any())
                {
                    allIsporuke = allIsporuke.OrderBy(isporuka => isporuka.DateIsporuka);

                    await allIsporuke.ForEachAsync(isporuka =>
                    {
                        if (_currentViewModel.AllIsporuke.FirstOrDefault(i => i.Id == isporuka.Id) == null)
                        {
                            Isporuka isp = new Isporuka(isporuka, _currentViewModel.CurrentDriver);
                            _currentViewModel.AllIsporuke.Add(isp);

                            _currentViewModel.TotalAmountIsporukefromDriver += isp.TotalAmount;
                        }
                    });
                }
                else
                {
                    MessageBox.Show("Izabran vozač nema isporuka.",
                        "Nema isporuka",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error("SearchCommand -> Execute -> Greska prilikom pretrage svih isporuka: ", ex);
                MessageBox.Show("Desila se neočekivana greška.\nObratite se serviseru.",
                    "Neočekivana greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}