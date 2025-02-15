using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database.Models;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Statistic.Refaund
{
    public class SearchRefaundInvoiceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RefaundViewModel _currentViewModel;

        public SearchRefaundInvoiceCommand(RefaundViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                _currentViewModel.AllInvoicesInDate = new List<Invoice>();
                var invoices = sqliteDbContext.Invoices.Where(invoice =>
                invoice.TransactionType == (int)UniversalEsir_Common.Enums.TransactionTypeEnumeration.Sale &&
                invoice.SdcDateTime.HasValue &&
                invoice.SdcDateTime.Value.Date == _currentViewModel.SelectedDateForRefund.Date);

                var invoicesRefaund = sqliteDbContext.Invoices.Where(invoice =>
                invoice.TransactionType == (int)UniversalEsir_Common.Enums.TransactionTypeEnumeration.Refund &&
                invoice.SdcDateTime.HasValue);

                if (invoices != null &&
                    invoices.Any())
                {
                    invoices.ToList().ForEach(invoice =>
                    {
                        if (invoicesRefaund.FirstOrDefault(inv => inv.ReferentDocumentNumber == invoice.InvoiceNumberResult) == null &&
                        !string.IsNullOrEmpty(invoice.InvoiceNumberResult))
                        {
                            _currentViewModel.AllInvoicesInDate.Add(new Invoice(invoice, _currentViewModel.AllInvoicesInDate.Count + 1));
                        }
                    });
                }
                _currentViewModel.SearchInvoices = new ObservableCollection<Invoice>(_currentViewModel.AllInvoicesInDate);

                _currentViewModel.InvoiceType = _currentViewModel.InvoiceType;
            }
            catch (Exception ex)
            {
                Log.Error("GRESKA U REFUNDACIJI", ex);
            }
        }
    }
}