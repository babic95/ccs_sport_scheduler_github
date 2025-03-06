using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir_Logging;
using System.Collections.ObjectModel;
using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir_Database.Models;
using UniversalEsir_Database;
using Microsoft.EntityFrameworkCore;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class OpenInsertIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenInsertIsporukaCommand(DriverViewModel currentViewModel)
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
                if (parameter != null &&
                    parameter is string)
                {
                    _currentViewModel.CurrentIsporuka = _currentViewModel.AllIsporuke.FirstOrDefault(isporuka =>
                    isporuka.Id == parameter.ToString());

                    if (_currentViewModel.CurrentIsporuka != null &&
                        _currentViewModel.CurrentDriver != null)
                    {
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        _currentViewModel.NeisporucenoDriverInvoices = new ObservableCollection<DriverInvoice>();

                        var invoices = sqliteDbContext.Invoices.Join(sqliteDbContext.DriverInvoices,
                        invoice => invoice.Id,
                        driverInvoice => driverInvoice.InvoiceId,
                        (invoice, driverInvoice) => new { Invoice = invoice, DriverInvoice = driverInvoice })
                        .Where(invoice => !string.IsNullOrEmpty(invoice.Invoice.InvoiceNumberResult) &&
                        (invoice.Invoice.InvoiceType == 0 || invoice.Invoice.InvoiceType == 5) &&
                        invoice.Invoice.TransactionType == 0 &&
                        invoice.DriverInvoice.DriverId == _currentViewModel.CurrentDriver.Id &&
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
                                        _currentViewModel.NeisporucenoDriverInvoices.Add(new DriverInvoice(invoice, _currentViewModel.CurrentIsporuka));
                                    }
                                }
                            });
                        }

                        _currentViewModel.WindowItemsInInvoice = new InsertIsporukaWindow(_currentViewModel);
                        _currentViewModel.WindowItemsInInvoice.ShowDialog();

                        _currentViewModel.WindowIsporuka.Close();

                        _currentViewModel.Initialize();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"OpenInsertIsporukaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}