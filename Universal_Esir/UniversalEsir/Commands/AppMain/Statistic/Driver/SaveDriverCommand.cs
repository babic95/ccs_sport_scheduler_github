using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database.Models;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SQLitePCL;
using Microsoft.EntityFrameworkCore;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class SaveDriverCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public SaveDriverCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.CurrentDriver == null ||
               _currentViewModel.CurrentDriver.Id == null ||
               _currentViewModel.CurrentDriver.Id < 1)
            {
                AddEditDriver();
            }
            else
            {
                AddEditDriver(_currentViewModel.CurrentDriver.Id);
            }
        }
        private void AddEditDriver(int? id = null)
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            if (_currentViewModel.CurrentDriver != null)
            {
                if (id == null)
                {
                    DriverDB driverDB = new DriverDB();
                    try
                    {
                        driverDB.Name = _currentViewModel.CurrentDriver.Name;
                        driverDB.Jmbg = _currentViewModel.CurrentDriver.Jmbg;
                        driverDB.Address = _currentViewModel.CurrentDriver.Address;
                        driverDB.City = _currentViewModel.CurrentDriver.City;
                        driverDB.ContractNumber = _currentViewModel.CurrentDriver.ContractNumber;
                        driverDB.Email = _currentViewModel.CurrentDriver.Email;

                        sqliteDbContext.Drivers.Add(driverDB);
                        sqliteDbContext.SaveChanges();

                        MessageBox.Show("Uspešno ste dodali vozača!", "Uspešno dodavanje", MessageBoxButton.OK, MessageBoxImage.Information);

                        _currentViewModel.Window.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom dodavanja vozača!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var result = MessageBox.Show("Da li ste sigurni da želite da izmenite vozača?", "Izmena vozača",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var driverDB = sqliteDbContext.Drivers.Find(id);

                            if (driverDB != null)
                            {
                                driverDB.Name = _currentViewModel.CurrentDriver.Name;
                                driverDB.Jmbg = _currentViewModel.CurrentDriver.Jmbg;
                                driverDB.Address = _currentViewModel.CurrentDriver.Address;
                                driverDB.City = _currentViewModel.CurrentDriver.City;
                                driverDB.ContractNumber = _currentViewModel.CurrentDriver.ContractNumber;
                                driverDB.Email = _currentViewModel.CurrentDriver.Email;

                                sqliteDbContext.Drivers.Update(driverDB);
                                sqliteDbContext.SaveChanges();

                                MessageBox.Show("Uspešno ste izmenili vozača!", "Uspešna izmena", MessageBoxButton.OK, MessageBoxImage.Information);

                                _currentViewModel.Window.Close();
                            }
                            else
                            {
                                MessageBox.Show("Ne postoji vozač!", "Ne postoji", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Greška prilikom izmene vozača!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            _currentViewModel.AllDrivers = new ObservableCollection<Models.AppMain.Statistic.Driver.Driver>();
            sqliteDbContext.Drivers.ForEachAsync(driver =>
            {
                bool hasDelevery = sqliteDbContext.DriverInvoices.Any(d => d.DriverId == driver.Id &&
                d.IsporukaId == null);

                _currentViewModel.AllDrivers.Add(new Models.AppMain.Statistic.Driver.Driver(driver, hasDelevery));
            });

            _currentViewModel.CurrentDriver = null;
        }
    }
}