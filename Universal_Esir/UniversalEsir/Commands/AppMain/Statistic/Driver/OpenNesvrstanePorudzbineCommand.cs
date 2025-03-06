using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
    public class OpenNesvrstanePorudzbineCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenNesvrstanePorudzbineCommand(DriverViewModel currentViewModel)
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
                _currentViewModel.AllNedodeljenePorudzbine = new ObservableCollection<DriverInvoice>();

                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var invoices = sqliteDbContext.Invoices.Join(sqliteDbContext.DriverInvoices,
                invoice => invoice.Id,
                driverInvoice => driverInvoice.InvoiceId,
                (invoice, driverInvoice) => new { Invoice = invoice, DriverInvoice = driverInvoice })
                    .Join(sqliteDbContext.Drivers,
                    invoice => invoice.DriverInvoice.DriverId,
                    driver => driver.Id,
                    (invoice, driver) => new {Invoice = invoice, Driver = driver})
                .Where(invoice => !string.IsNullOrEmpty(invoice.Invoice.Invoice.InvoiceNumberResult) &&
                (invoice.Invoice.Invoice.InvoiceType == 0 || invoice.Invoice.Invoice.InvoiceType == 5) && 
                invoice.Invoice.Invoice.TransactionType == 0)
                .Select(invoice => invoice.Invoice.Invoice);

                var nesvrstanePorudzbine = sqliteDbContext.Invoices.Where(invoice => !string.IsNullOrEmpty(invoice.InvoiceNumberResult) &&
                (invoice.InvoiceType == 0 || invoice.InvoiceType == 5) &&
                invoice.TransactionType == 0)
                    .Except(invoices);

                if(nesvrstanePorudzbine != null &&
                    nesvrstanePorudzbine.Any())
                {
                    nesvrstanePorudzbine = nesvrstanePorudzbine.OrderByDescending(invoice => invoice.SdcDateTime);
                }

                var refundInvoices = sqliteDbContext.Invoices.Where(invoice => invoice.TransactionType == 1 &&
                (invoice.InvoiceType == 0  || invoice.InvoiceType == 5) &&
                !string.IsNullOrEmpty(invoice.InvoiceNumberResult));

                int index = 1;
                if (nesvrstanePorudzbine != null &&
                    nesvrstanePorudzbine.Any())
                {
                    await nesvrstanePorudzbine.ForEachAsync(invoiceDB =>
                    {
                        if (invoiceDB.TotalAmount.HasValue)
                        {
                            var refundInvoice = refundInvoices.FirstOrDefault(invoice =>
                            invoice.ReferentDocumentNumber == invoiceDB.InvoiceNumberResult);

                            if (refundInvoice == null)
                            {
                                Models.Sale.Invoice invoice = new Models.Sale.Invoice(invoiceDB, index++);
                                DriverInvoice driverInvoice = new DriverInvoice(invoice);
                                _currentViewModel.AllNedodeljenePorudzbine.Add(driverInvoice);
                            }
                        }
                    });

                    _currentViewModel.WindowIsporuka = new NesvrstanePorudzbineWindow(_currentViewModel);
                    _currentViewModel.WindowIsporuka.ShowDialog();

                    _currentViewModel.Initialize();
                }
            }
            catch(Exception ex)
            {
                Log.Error($"OpenNesvrstanePorudzbineCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}