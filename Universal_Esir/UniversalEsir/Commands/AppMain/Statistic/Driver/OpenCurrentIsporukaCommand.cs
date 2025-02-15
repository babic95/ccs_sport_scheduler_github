using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database.Models;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir_Logging;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class OpenCurrentIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenCurrentIsporukaCommand(DriverViewModel currentViewModel)
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
                    int driverId = Convert.ToInt32(parameter);

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var driverDB = await sqliteDbContext.Drivers.FindAsync(driverId);

                    if (driverDB == null)
                    {
                        Log.Error($"OpenAllIsporukaCommand -> Execute -> Vozac sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Vozač ne postoji u bazi podataka.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var driver = _currentViewModel.AllDrivers.FirstOrDefault(driver => driver.Id == driverId);

                    if (driver == null)
                    {
                        Log.Error($"OpenAllIsporukaCommand -> Execute -> Vozac sa ID = {parameter} ne postoji u tabeli!");
                        MessageBox.Show("Vozač ne postoji u tabeli.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    _currentViewModel.CurrentIsporuka = new Isporuka()
                    {
                        Driver = driver,
                        CreateDate = DateTime.Now,
                        Id = Guid.NewGuid().ToString(),
                        TotalAmount = 0,
                        DriverInvoices = new ObservableCollection<DriverInvoice>()
                    };

                    var invoices = sqliteDbContext.Invoices.Join(sqliteDbContext.DriverInvoices,
                    invoice => invoice.Id,
                    driverInvoice => driverInvoice.InvoiceId,
                    (invoice, driverInvoice) => new { Invoice = invoice, DriverInvoice = driverInvoice })
                    .Where(invoice => !string.IsNullOrEmpty(invoice.Invoice.InvoiceNumberResult) &&
                    (invoice.Invoice.InvoiceType == 0 || invoice.Invoice.InvoiceType == 5)
                    && invoice.Invoice.TransactionType == 0 &&
                    invoice.DriverInvoice.DriverId == driverDB.Id &&
                    invoice.DriverInvoice.IsporukaId == null)
                    .Select(invoice => invoice.Invoice);

                    var refundInvoices = sqliteDbContext.Invoices.Where(invoice => invoice.TransactionType == 1 &&
                    (invoice.InvoiceType == 0 || invoice.InvoiceType == 5) &&
                    !string.IsNullOrEmpty(invoice.InvoiceNumberResult));

                    int index = 1;
                    if (invoices != null &&
                        invoices.Any())
                    {
                        await invoices.ForEachAsync(invoiceDB =>
                        {
                            if (invoiceDB.TotalAmount.HasValue)
                            {
                                var refundInvoice = refundInvoices.FirstOrDefault(invoice => 
                                invoice.ReferentDocumentNumber == invoiceDB.InvoiceNumberResult);

                                if (refundInvoice == null)
                                {
                                    Models.Sale.Invoice invoice = new Models.Sale.Invoice(invoiceDB, index++);
                                    _currentViewModel.CurrentIsporuka.DriverInvoices.Add(new DriverInvoice(invoice, _currentViewModel.CurrentIsporuka));
                                }
                            }
                        });

                        _currentViewModel.IsAllSelected = true;

                        _currentViewModel.WindowIsporuka = new CreateIsporukaWindow(_currentViewModel);
                        _currentViewModel.WindowIsporuka.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"OpenAllIsporukaCommand -> Execute -> Greska prilikom obrade Id = {parameter}");
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}