using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Common.Models.Statistic.Driver;
using UniversalEsir_Database;
using UniversalEsir_Logging;
using UniversalEsir_Printer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class PrintIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public PrintIsporukaCommand(DriverViewModel currentViewModel)
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
                        Log.Error($"PrintIsporukaCommand -> Execute -> Isporuka sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Isporuka ne postoji u bazi podataka.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var driverInvoicesDB = sqliteDbContext.Invoices.Join(sqliteDbContext.DriverInvoices,
                        invoice => invoice.Id,
                        driverInvoice => driverInvoice.InvoiceId,
                        (invoice, driverInvoice) => new { Invoice = invoice, DriverInvoice = driverInvoice })
                        .Where(invoice => invoice.DriverInvoice.IsporukaId == isporukaDB.Id);

                    if (driverInvoicesDB == null)
                    {
                        Log.Error($"PrintIsporukaCommand -> Execute -> Greska u obradi DriverInvoices");
                        MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var driver = driverInvoicesDB.FirstOrDefault();

                    if (driver == null)
                    {
                        Log.Error($"PrintIsporukaCommand -> Execute -> Ne postoji Driver");
                        MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var driverDB = await sqliteDbContext.Drivers.FindAsync(driver.DriverInvoice.DriverId);

                    if (driverDB == null)
                    {
                        Log.Error($"PrintIsporukaCommand -> Execute -> Ne postoji Driver u bazi podataka");
                        MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    IsporukaGlobal isporukaGlobal = new IsporukaGlobal()
                    {
                        Counter = isporukaDB.Counter.ToString(),
                        DateCreate = isporukaDB.CreateDate.ToString("dd.MM.yyyy"),
                        DateIsporuke = isporukaDB.DateIsporuka.ToString("dd.MM.yyyy"),
                        TotalAmount = string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(isporukaDB.TotalAmount), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.'),
                        IsporukaName = $"Isporuka_{isporukaDB.Counter}",
                        DriverInvoiceGlobals = new List<DriverInvoiceGlobal>(),
                        Driver = new DriverGlobal()
                        {
                            Id = driverDB.Id,
                            Name = driverDB.Name,
                            ContractNumber = driverDB.ContractNumber,
                            Email = driverDB.Email
                        }
                    };

                    await driverInvoicesDB.ForEachAsync(driverInvoice =>
                    {
                        DriverInvoiceGlobal driverInvoiceGlobal = new DriverInvoiceGlobal()
                        {
                            Porudzbenica = driverInvoice.Invoice.Porudzbenica,
                            InvoiceNumber = driverInvoice.Invoice.InvoiceNumberResult,
                            Date = driverInvoice.Invoice.SdcDateTime.Value.ToString("dd.MM.yyyy"),
                            TotalAmount = string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(driverInvoice.Invoice.TotalAmount), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.')
                        };
                        isporukaGlobal.DriverInvoiceGlobals.Add(driverInvoiceGlobal);
                    });

                    PrinterManager.Instance.PrintIsporuku(isporukaGlobal);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PrintIsporukaCommand -> Execute -> Greska prilikom stampe isporuke", ex);
                MessageBox.Show("Greška prilikom štampe isporuke.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}