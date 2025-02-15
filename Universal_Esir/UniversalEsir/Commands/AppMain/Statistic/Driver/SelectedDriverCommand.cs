using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
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
    public class SelectedDriverCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public SelectedDriverCommand(DriverViewModel currentViewModel)
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
                if (_currentViewModel.CurrentNedodeljenaPorudzbina != null)
                {
                    var result = MessageBox.Show("Da li zaista želite da dodate vozača porudzbini?",
                        "Dodavanje vozača porudzbini",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (_currentViewModel.CurrentDriver == null)
                        {
                            Log.Error($"SelectedDriverCommand -> Execute -> Vozac je null!");
                            MessageBox.Show("Vozač ne postoji u bazi podataka.\nObratite se serviseru.",
                                "Greška",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        var driverDB = await sqliteDbContext.Drivers.FindAsync(_currentViewModel.CurrentDriver.Id);

                        if (driverDB == null)
                        {
                            Log.Error($"SelectedDriverCommand -> Execute -> Vozac sa ID = {_currentViewModel.CurrentDriver.Id} ne postoji u bazi!");
                            MessageBox.Show("Vozač ne postoji u bazi podataka.\nObratite se serviseru.",
                                "Greška",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        DriverInvoiceDB driverInvoiceDB = new DriverInvoiceDB()
                        {
                            DriverId = driverDB.Id,
                            InvoiceId = _currentViewModel.CurrentNedodeljenaPorudzbina.Invoice.Id,
                            IsporukaId = null
                        };

                        sqliteDbContext.DriverInvoices.Add(driverInvoiceDB);
                        sqliteDbContext.SaveChanges();

                        _currentViewModel.Window.Close();
                    }
                }
                else
                {
                    if (_currentViewModel.CurrentPorudzbina != null)
                    {
                        var result = MessageBox.Show("Da li zaista želite da izmenite vozača porudzbini?",
                            "Izmena vozača porudzbini",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            if (_currentViewModel.CurrentDriver == null)
                            {
                                Log.Error($"SelectedDriverCommand -> Execute -> else -> Vozac je null!");
                                MessageBox.Show("Vozač ne postoji u bazi podataka.\nObratite se serviseru.",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            SqliteDbContext sqliteDbContext = new SqliteDbContext();

                            var driverDB = await sqliteDbContext.Drivers.FindAsync(_currentViewModel.CurrentDriver.Id);

                            if (driverDB == null)
                            {
                                Log.Error($"SelectedDriverCommand -> Execute -> else -> Vozac sa ID = {_currentViewModel.CurrentDriver.Id} ne postoji u bazi!");
                                MessageBox.Show("Vozač ne postoji u bazi podataka.\nObratite se serviseru.",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            var driverInvoiceDB = sqliteDbContext.DriverInvoices.FirstOrDefault(invoice => invoice.InvoiceId == _currentViewModel.CurrentPorudzbina.Invoice.Id);

                            if (driverInvoiceDB != null &&
                                driverInvoiceDB.DriverId != _currentViewModel.CurrentDriver.Id)
                            {
                                sqliteDbContext.DriverInvoices.Remove(driverInvoiceDB);
                                sqliteDbContext.SaveChanges();

                                driverInvoiceDB.DriverId = driverDB.Id;

                                sqliteDbContext.DriverInvoices.Add(driverInvoiceDB);
                                sqliteDbContext.SaveChanges();
                            }

                            _currentViewModel.Window.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SelectedDriverCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}