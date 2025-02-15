using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Database.Models;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace UniversalEsir.Commands.AppMain.Statistic.Otpremnice
{
    public class SearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpremniceViewModel _currentViewModel;

        public SearchCommand(OtpremniceViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _currentViewModel.TotalAmount = 0;

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            var otpremniceRefund = sqliteDbContext.Invoices.Where(otpremnica => otpremnica.InvoiceType == 5 &&
            otpremnica.TransactionType == 1 &&
            !string.IsNullOrEmpty(otpremnica.InvoiceNumberResult));

            otpremniceRefund.ForEachAsync(refundOtpremnica =>
            {
                var invoice = sqliteDbContext.Invoices.FirstOrDefault(invo => invo.InvoiceNumberResult == refundOtpremnica.ReferentDocumentNumber);

                if (invoice != null)
                {
                    var driverInvoice = sqliteDbContext.DriverInvoices.FirstOrDefault(di => di.InvoiceId == invoice.Id);

                    if (driverInvoice != null)
                    {
                        sqliteDbContext.DriverInvoices.Remove(driverInvoice);
                    }
                }
            });

            sqliteDbContext.SaveChanges();

            _currentViewModel.Otpremnice = new List<Models.Sale.Invoice>();

            var otpremnice = sqliteDbContext.Invoices.Where(otpremnica => otpremnica.InvoiceType == 5 &&
            otpremnica.TransactionType == 0 &&
            !string.IsNullOrEmpty(otpremnica.InvoiceNumberResult) &&
            otpremnica.SdcDateTime.HasValue &&
            otpremnica.SdcDateTime.Value.Date >= _currentViewModel.StartDate.Date &&
            otpremnica.SdcDateTime.Value.Date <= _currentViewModel.EndDate.Date);

            int index = 1;
            if (otpremnice != null &&
                otpremnice.Any())
            {
                otpremnice.ForEachAsync(otpremnicaDB =>
                {
                    if (otpremnicaDB.TotalAmount.HasValue)
                    {
                        var refundOtpremnica = otpremniceRefund.FirstOrDefault(otpremnica =>
                        otpremnica.ReferentDocumentNumber == otpremnicaDB.InvoiceNumberResult);

                        if (refundOtpremnica == null)
                        {
                            Models.Sale.Invoice otpremnica = new Models.Sale.Invoice(otpremnicaDB, index++);
                            _currentViewModel.Otpremnice.Add(otpremnica);

                            _currentViewModel.TotalAmount += otpremnicaDB.TotalAmount.Value;
                        }
                    }
                });
            }

            _currentViewModel.AllOtpremnice = new ObservableCollection<Models.Sale.Invoice>(_currentViewModel.Otpremnice);
        }
    }
}