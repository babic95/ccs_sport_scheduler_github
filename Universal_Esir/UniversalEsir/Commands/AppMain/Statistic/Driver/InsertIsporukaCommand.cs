using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;
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
using UniversalEsir.Models.Sale;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class InsertIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public InsertIsporukaCommand(DriverViewModel currentViewModel)
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
                var result = MessageBox.Show("Da li zaista želite da dodate porudzbinu u ovu isporuku?",
                        "Dodavanje porudzbine",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var invoices = _currentViewModel.NeisporucenoDriverInvoices.Where(invoice => invoice.IsChecked);

                    if (invoices != null &&
                        invoices.Any())
                    {
                        invoices.ToList().ForEach(invoice =>
                        {
                            var driveInvoiceDB = sqliteDbContext.DriverInvoices.FirstOrDefault(inv => inv.InvoiceId == invoice.Invoice.Id);

                            if (driveInvoiceDB != null)
                            {
                                var isporukaDB = sqliteDbContext.Isporuke.Find(_currentViewModel.CurrentIsporuka.Id);

                                if (isporukaDB != null)
                                {
                                    isporukaDB.TotalAmount += invoice.Invoice.TotalAmount;

                                    driveInvoiceDB.IsporukaId = isporukaDB.Id;

                                    sqliteDbContext.Isporuke.Update(isporukaDB);
                                    sqliteDbContext.DriverInvoices.Update(driveInvoiceDB);
                                }
                            }
                        });

                        MessageBox.Show("Usprešno dodate porudzbine u isporuku!",
                                "Usprešno",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                    }

                    sqliteDbContext.SaveChanges();

                    _currentViewModel.WindowItemsInInvoice.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"InsertIsporukaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}