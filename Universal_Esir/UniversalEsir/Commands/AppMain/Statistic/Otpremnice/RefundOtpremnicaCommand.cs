using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.ViewModels.Sale;
using UniversalEsir_Common.Enums;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Printer;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
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
    public class RefundOtpremnicaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpremniceViewModel _currentViewModel;

        public RefundOtpremnicaCommand(OtpremniceViewModel currentViewModel)
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

                    var otpremnicaDB = sqliteDbContext.Invoices.Find(parameter.ToString());

                    if (otpremnicaDB == null)
                    {
                        Log.Error($"RefundOtpremnicaCommand -> Execute -> Greska prilikom refundacije otpremnice, " +
                            $"ne postoji u bazi sa Id={parameter.ToString()}");
                        MessageBox.Show("Greška prilikom refundacije otpremnice.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    var result = MessageBox.Show("Da li zaista želite da stornirate otpremnicu?",
                        "Storniranje otpremnice",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {

                        DateTime dateTimeOfIssue = DateTime.Now;

                        InvoiceDB invoiceDB = new InvoiceDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Cashier = otpremnicaDB.Cashier,
                            InvoiceType = 5,
                            TransactionType = 1,
                            SdcDateTime = dateTimeOfIssue,
                            TotalAmount = otpremnicaDB.TotalAmount,
                            InvoiceNumberResult = $"{otpremnicaDB.InvoiceNumberResult}-refundirano",
                            Porudzbenica = otpremnicaDB.Porudzbenica,
                            DateAndTimeOfIssue = dateTimeOfIssue,
                            BuyerId = otpremnicaDB.BuyerId,
                            BuyerName = otpremnicaDB.BuyerName,
                            BuyerAddress = otpremnicaDB.BuyerAddress,
                            ReferentDocumentDt = otpremnicaDB.SdcDateTime,
                            ReferentDocumentNumber = otpremnicaDB.InvoiceNumberResult
                        };
                        sqliteDbContext.Add(invoiceDB);

                        sqliteDbContext.SaveChanges();

                        List<UniversalEsir_Common.Models.Invoice.Item> itemsOtpremnica = new List<UniversalEsir_Common.Models.Invoice.Item>();

                        var items = sqliteDbContext.ItemInvoices.Where(item => item.InvoiceId == otpremnicaDB.Id);

                        if (items != null &&
                            items.Any())
                        {

                            int itemInvoiceId = 0;
                            items.ForEachAsync(item =>
                            {
                                ItemDB? itemDB = sqliteDbContext.Items.Find(item.ItemCode);
                                if (itemDB != null)
                                {
                                    ItemInvoiceDB itemInvoiceDB = new ItemInvoiceDB()
                                    {
                                        Id = itemInvoiceId++,
                                        Quantity = item.Quantity,
                                        TotalAmout = item.TotalAmout,
                                        Label = item.Label,
                                        Name = item.Name,
                                        UnitPrice = item.UnitPrice,
                                        ItemCode = item.ItemCode,
                                        InvoiceId = invoiceDB.Id
                                        //Item = itemDB
                                    };

                                    sqliteDbContext.Add(itemInvoiceDB);

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
                                }
                            });
                            sqliteDbContext.SaveChanges();
                        }

                        PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                        {
                            InvoiceId = invoiceDB.Id,
                            Amout = otpremnicaDB.TotalAmount,
                            PaymentType = PaymentTypeEnumeration.Otpremnica
                        };

                        sqliteDbContext.PaymentInvoices.Add(paymentInvoice);

                        sqliteDbContext.SaveChanges();

                        Otpremnica otpremnica = new Otpremnica()
                        {
                            Porudzbenica = invoiceDB.Porudzbenica,
                            SdcDateTime = invoiceDB.SdcDateTime.Value,
                            BuyerId = invoiceDB.BuyerId,
                            BuyerName = invoiceDB.BuyerName,
                            BuyerAddress = invoiceDB.BuyerAddress,
                            InvoiceNumberResult = invoiceDB.InvoiceNumberResult,
                            TotalAmount = invoiceDB.TotalAmount.Value,
                        };
                        otpremnica.Items = itemsOtpremnica;

                        PrinterManager.Instance.PrintVirman(otpremnica);
                    }
                }
                _currentViewModel.Initialize();
            }
            catch (Exception ex)
            {
                Log.Error($"RefundOtpremnicaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}