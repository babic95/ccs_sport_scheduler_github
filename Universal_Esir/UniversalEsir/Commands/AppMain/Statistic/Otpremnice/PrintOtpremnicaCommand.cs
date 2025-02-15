using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Printer;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Otpremnice
{
    public class PrintOtpremnicaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpremniceViewModel _currentViewModel;

        public PrintOtpremnicaCommand(OtpremniceViewModel currentViewModel)
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
                if (parameter != null &&
                    parameter is string)
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var otpremnica = _currentViewModel.Otpremnice.FirstOrDefault(otp => otp.Id == parameter.ToString());

                    if (otpremnica != null)
                    {
                        var itemsInOptremnica = sqliteDbContext.ItemInvoices.Where(item => item.InvoiceId == otpremnica.Id);

                        if (itemsInOptremnica != null &&
                            itemsInOptremnica.Any())
                        {
                            List<UniversalEsir_Common.Models.Invoice.Item> itemsOtpremnica = new List<UniversalEsir_Common.Models.Invoice.Item>();

                            itemsInOptremnica.ForEachAsync(item =>
                            {
                                var itemDB = sqliteDbContext.Items.Find(item.ItemCode);

                                if (itemDB != null &&
                                item.UnitPrice.HasValue &&
                                item.Quantity.HasValue &&
                                item.TotalAmout.HasValue &&
                                !string.IsNullOrEmpty(item.Name))
                                {
                                    itemsOtpremnica.Add(new UniversalEsir_Common.Models.Invoice.Item()
                                    {
                                        Name = item.Name,
                                        UnitPrice = item.UnitPrice.Value,
                                        Quantity = item.Quantity.Value,
                                        TotalAmount = item.TotalAmout.Value,
                                        Jm = itemDB.Jm,
                                    });
                                }
                            });

                            Otpremnica otp = new Otpremnica()
                            {
                                Porudzbenica = otpremnica.Porudzbenica,
                                SdcDateTime = otpremnica.SdcDateTime,
                                BuyerId = otpremnica.BuyerId,
                                BuyerName = otpremnica.BuyerName,
                                BuyerAddress = otpremnica.BuyerAddress,
                                InvoiceNumberResult = otpremnica.InvoiceNumber,
                                TotalAmount = otpremnica.TotalAmount,
                            };
                            otp.Items = itemsOtpremnica;

                            //var firma = sqliteDbContext.Firmas.FirstOrDefault();
                            //if (firma != null )
                            //{
                            //    otpremnica.Tin = firma.Pib;
                            //    otpremnica.BusinessName = firma.Name;
                            //    otpremnica.Address = firma.AddressPP;
                            //    otpremnica.MB = firma.MB;
                            //    otpremnica.BankAccount = firma.BankAcc;
                            //}

                            PrinterManager.Instance.PrintVirman(otp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PrintOtpremnicaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}