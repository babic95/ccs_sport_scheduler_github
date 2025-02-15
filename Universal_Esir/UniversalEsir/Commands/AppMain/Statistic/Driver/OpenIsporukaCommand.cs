using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir_Logging;
using Microsoft.EntityFrameworkCore;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class OpenIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenIsporukaCommand(DriverViewModel currentViewModel)
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
                if (parameter != null)
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var isporukaDB = await sqliteDbContext.Isporuke.FindAsync(parameter.ToString());

                    if (isporukaDB == null)
                    {
                        Log.Error($"OpenIsporukaCommand -> Execute -> Isporuka sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Isporuka ne postoji u bazi podataka.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var driverDB = await sqliteDbContext.Drivers.Join(sqliteDbContext.DriverInvoices,
                        driver => driver.Id,
                        driverInvoice => driverInvoice.DriverId,
                        (driver, driverInvoice) => new { Driver = driver, DriverInvoice = driverInvoice })
                        .FirstOrDefaultAsync(driver => driver.DriverInvoice.IsporukaId == isporukaDB.Id);

                    if (driverDB == null)
                    {
                        Log.Error($"OpenIsporukaCommand -> Execute -> Vozac u Isporuci sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Isporuka ne postoji u bazi podataka.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    Models.AppMain.Statistic.Driver.Driver driver = new Models.AppMain.Statistic.Driver.Driver(driverDB.Driver, false);

                    _currentViewModel.CurrentIsporuka = new Isporuka(isporukaDB, driver);

                    _currentViewModel.Window = new InvoiceInIsporukaWindow(_currentViewModel);
                    _currentViewModel.Window.ShowDialog();
                }
            }
            catch(Exception ex)
            {
                Log.Error($"OpenIsporukaCommand -> Execute -> Neocekivana greska: ", ex);
                MessageBox.Show("Desila se neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}