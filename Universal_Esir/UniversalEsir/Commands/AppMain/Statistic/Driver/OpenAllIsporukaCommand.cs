using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
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
    public class OpenAllIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenAllIsporukaCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            _currentViewModel.AllIsporuke = new ObservableCollection<Isporuka>();
            _currentViewModel.TotalAmountIsporukefromDriver = 0;

            if(parameter != null)
            {
                try
                {
                    int driverId = Convert.ToInt32(parameter);

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var driverDB = await sqliteDbContext.Drivers.FindAsync(driverId);

                    if(driverDB == null)
                    {
                        Log.Error($"OpenAllIsporukaCommand -> Execute -> Vozac sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Vozač ne postoji u bazi podataka.\nObratite se serviseru.",
                            "Greška", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return;
                    }

                    var driver = _currentViewModel.AllDrivers.FirstOrDefault(driver => driver.Id == driverId);

                    if(driver == null )
                    {
                        Log.Error($"OpenAllIsporukaCommand -> Execute -> Vozac sa ID = {parameter} ne postoji u tabeli!");
                        MessageBox.Show("Vozač ne postoji u tabeli.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    _currentViewModel.CurrentDriver = driver;

                    var allIsporuke = sqliteDbContext.Isporuke.Join(sqliteDbContext.DriverInvoices,
                        isporuke => isporuke.Id,
                        driverInvoice => driverInvoice.IsporukaId,
                        (isporuke, driverInvoice) => new { Isporuka = isporuke, DriverInvoice = driverInvoice })
                        .Where(isporuke => isporuke.DriverInvoice.DriverId == driver.Id &&
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
                                Isporuka isp = new Isporuka(isporuka, driver);
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

                    _currentViewModel.WindowIsporuka = new AllIsporukeWindow(_currentViewModel);
                    _currentViewModel.WindowIsporuka.ShowDialog();

                    _currentViewModel.StartDate = _currentViewModel.EndDate = DateTime.Now;
                    _currentViewModel.StartDate = new DateTime(_currentViewModel.EndDate.Year, 1, 1);
                }
                catch(Exception ex)
                {
                    Log.Error($"OpenAllIsporukaCommand -> Execute -> Greska prilikom otvaranja svih isporuka od vozaca sa ID = {parameter}", ex);
                }
            }
        }
    }
}