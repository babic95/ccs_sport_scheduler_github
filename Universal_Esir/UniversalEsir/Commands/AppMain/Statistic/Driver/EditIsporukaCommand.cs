using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class EditIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public EditIsporukaCommand(DriverViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li zaista želite da izmenite isporuku?",
                        "Izmena isporuke",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var invoice = _currentViewModel.CurrentIsporuka.DriverInvoices.Where(invoice => invoice.IsChecked == false);

                    if (invoice != null &&
                        invoice.Any())
                    {
                        invoice.ToList().ForEach(invoice =>
                        {
                            var driverInvoiceDB = sqliteDbContext.DriverInvoices.FirstOrDefault(inv => inv.InvoiceId == invoice.Invoice.Id);

                            if (driverInvoiceDB != null &&
                            driverInvoiceDB.IsporukaId != null)
                            {
                                var isporukaDB = sqliteDbContext.Isporuke.Find(driverInvoiceDB.IsporukaId);

                                if (isporukaDB != null)
                                {
                                    isporukaDB.TotalAmount -= invoice.Invoice.TotalAmount;
                                    sqliteDbContext.Isporuke.Update(isporukaDB);
                                }

                                driverInvoiceDB.IsporukaId = null;
                                sqliteDbContext.DriverInvoices.Update(driverInvoiceDB);
                            }
                        });
                    }

                    sqliteDbContext.SaveChanges();

                    _currentViewModel.WindowItemsInInvoice.Close();

                    MessageBox.Show("Usprešno ste izmenili isporuku!",
                            "Usprešno izmenjena isporuku",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"OpenEditIsporukaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}