using UniversalEsir.Converters;
using UniversalEsir.Enums.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.Sale;
using UniversalEsir.Views.Sale.PaySale;
using UniversalEsir_Common.Enums;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Common.Models.Invoice.FileSystemWatcher;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Printer;
using UniversalEsir_Settings;
using DocumentFormat.OpenXml.Vml;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DocumentFormat.OpenXml.Bibliography;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Sale;
using UniversalEsir_SportSchedulerAPI.RequestModel.Racun;
using UniversalEsir_SportSchedulerAPI;
using UniversalEsir_SportSchedulerAPI.RequestModel.Zaduzenje;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir.Enums;

namespace UniversalEsir.Commands.Sale
{
    public class PayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _viewModel;

        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private DateTime _timer;

        public PayCommand(ViewModelBase viewModel)
        {
          _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public async void Execute(object parameter)
        {
            if(_viewModel is SaleViewModel)
            {
                SaleViewModel saleViewModel = (SaleViewModel)_viewModel;
                if (saleViewModel.ItemsInvoice.Any() ||
                    saleViewModel.OldOrders.Any())
                {

                    foreach (var item in saleViewModel.OldOrders)
                    {
                        var itemInvoice = saleViewModel.ItemsInvoice.FirstOrDefault(itemInvoice => itemInvoice.Item.Id == item.Item.Id);

                        if (itemInvoice != null)
                        {
                            itemInvoice.Quantity += item.Quantity;
                            itemInvoice.TotalAmout += item.TotalAmout;
                        }
                        else
                        {
                            saleViewModel.ItemsInvoice.Add(item);
                        }
                    }


                    PaySaleWindow paySaleWindow = new PaySaleWindow(saleViewModel);
                    paySaleWindow.ShowDialog();
                }
            }
            else if(_viewModel is PaySaleViewModel)
            {
                if(parameter is not string)
                {
                    return;
                }

                string paymentType = parameter as string;

                PaySaleViewModel paySaleViewModel = (PaySaleViewModel)_viewModel;

                if(paySaleViewModel.SaleViewModel.CurrentClan == null)
                {
                    MessageBox.Show("Niste izabrali člana!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                paySaleViewModel.ChangeFocusCommand.Execute("Pay");

                if (paySaleViewModel.Amount < paySaleViewModel.TotalAmount &&
                    paySaleViewModel.InvoiceType != Enums.Sale.InvoiceTypeEnumeration.Predračun)
                {
                    MessageBox.Show("Uplata nije dobra!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    paySaleViewModel.Focus = FocusEnumeration.Cash;
                    return;
                }

                //if (paySaleViewModel.SaleViewModel.InvoiceType == Enums.InvoiceTypeEnumeration.Avans &&
                //    paySaleViewModel.Amount < 1m)
                //{
                //    MessageBox.Show("Vrednost uplate avansa mora biti minimum 1!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.Cash;
                //    return;
                //}

                //if (paySaleViewModel.SaleViewModel.InvoiceType != Enums.InvoiceTypeEnumeration.Avans &&
                //    paySaleViewModel.Amount < paySaleViewModel.SaleViewModel.TotalAmount)
                //{
                //    paySaleViewModel.Focus = ViewModels.AppMain.Sale.FocusEnumeration.Cash;
                //    return;
                //}


                decimal povecajCenu = 0;

                AddPayment(paySaleViewModel, paymentType);

                if (paySaleViewModel.Payment == null)
                {
                    MessageBox.Show("Niste uneli način plaćanja",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                await FinisedSale(paySaleViewModel, povecajCenu);

                paySaleViewModel.Window.Close();
            }
            else if (_viewModel is SplitOrderViewModel)
            {
                SplitOrderViewModel splitOrderViewModel = (SplitOrderViewModel)_viewModel;

                splitOrderViewModel.Window.Close();

                if (splitOrderViewModel.ItemsInvoiceForPay.Any())
                {
                    splitOrderViewModel.PaySaleViewModel.ItemsInvoice = splitOrderViewModel.ItemsInvoiceForPay;
                    splitOrderViewModel.PaySaleViewModel.TotalAmount = splitOrderViewModel.TotalAmountForPay;
                }

                if (splitOrderViewModel.ItemsInvoice.Any())
                {
                    splitOrderViewModel.PaySaleViewModel.SaleViewModel.ItemsInvoice = splitOrderViewModel.ItemsInvoice;
                    splitOrderViewModel.PaySaleViewModel.SaleViewModel.TotalAmount = splitOrderViewModel.TotalAmount;
                }
            }
        }
        private void AddPayment(PaySaleViewModel paySaleViewModel, string paymentType)
        {
            paySaleViewModel.Payment = null;
            if (!string.IsNullOrEmpty(paymentType))
            {
                switch (paymentType)
                {
                    case "Platio":
                        paySaleViewModel.Payment = new Payment()
                        {
                            Amount = 0,
                            PaymentType = PaymentTypeEnumeration.Cash,
                        };
                        break;
                    case "Crta":
                        paySaleViewModel.Payment = new Payment()
                        {
                            Amount = 0,
                            PaymentType = PaymentTypeEnumeration.Crta,
                        };
                        break;
                }
            }
        }
        private async Task FinisedSale(PaySaleViewModel paySaleViewModel, decimal vecaCena)
        {
            InvoceRequestFileSystemWatcher invoiceRequset = new InvoceRequestFileSystemWatcher()
            {
                ReferentDocumentNumber = paySaleViewModel.LastAdvanceInvoice,
                Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Name,
                InvoiceType = (InvoiceTypeEenumeration)paySaleViewModel.InvoiceType,
                TransactionType = TransactionTypeEnumeration.Sale,
                BuyerId = string.IsNullOrEmpty(paySaleViewModel.BuyerId) == true ? null :
                $"{paySaleViewModel.CurrentBuyerIdElement.Id}:{paySaleViewModel.BuyerId}",
                BuyerAddress = string.IsNullOrEmpty(paySaleViewModel.BuyerAdress) == true ? null :
                paySaleViewModel.BuyerAdress,
                BuyerName = string.IsNullOrEmpty(paySaleViewModel.BuyerName) == true ? null :
                paySaleViewModel.BuyerName,
                Payment = paySaleViewModel.Payment
            };

            List<ItemFileSystemWatcher> items = new List<ItemFileSystemWatcher>();

            decimal total = 0;

            paySaleViewModel.ItemsInvoice.ToList().ForEach(item =>
            {
                if(vecaCena > 0 &&
                !item.Item.Name.ToLower().Contains("kotizacija"))
                {
                    item.Item.SellingUnitPrice = Decimal.Round(item.Item.SellingUnitPrice + vecaCena, 2);
                }

                ItemFileSystemWatcher itemFileSystemWatcher = new ItemFileSystemWatcher()
                {
                    Id = item.Item.Id,
                    Label = item.Item.Label,
                    Name = $"{item.Item.Name}",
                    UnitPrice = item.Item.SellingUnitPrice,
                    Quantity = item.Quantity,
                    TotalAmount = Decimal.Round(item.Item.SellingUnitPrice * item.Quantity, 2),
                    Jm = item.Item.Jm
                };

                items.Add(itemFileSystemWatcher);
                total += itemFileSystemWatcher.TotalAmount;
            });
            invoiceRequset.Items = items;

            invoiceRequset.Payment.Amount = total;
#if CRNO
            Black(invoiceRequset, paySaleViewModel, total, items);
#else
            var paymentType = invoiceRequset.Payment.FirstOrDefault(pay => pay.PaymentType == PaymentTypeEnumeration.Otpremnica ||
            pay.PaymentType == PaymentTypeEnumeration.Ponuda);
            if (paymentType != null)
            {
                if (paymentType.PaymentType == PaymentTypeEnumeration.Otpremnica)
                {
                    BlackOtpremnica(invoiceRequset,
                        paySaleViewModel,
                        total,
                        items,
                        paySaleViewModel.Porudzbenica,
                        paySaleViewModel.CurrentDriver);
                }
                else if(paymentType.PaymentType == PaymentTypeEnumeration.Ponuda)
                {
                    CreatePonuda(invoiceRequset,
                        paySaleViewModel,
                        total,
                        items);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(paySaleViewModel.Porudzbenica))
                {
                    invoiceRequset.BuyerName = paySaleViewModel.Porudzbenica;
                }

                Normal(invoiceRequset, paySaleViewModel, total, items, popust);
            }
#endif
        }
        private async Task TakingDownOrder(InvoiceDB invoice,
            PaySaleViewModel paySaleViewModel,
            int tableId,
            ObservableCollection<ItemInvoice> items)
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            var unprocessedOrders = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == tableId);

            if (unprocessedOrders != null)
            {
                OrderDB orderDB = new OrderDB()
                {
                    InvoiceId = invoice.Id,
                    PaymentPlaceId = tableId,
                    CashierId = paySaleViewModel.SaleViewModel.LoggedCashier.Id
                };
                sqliteDbContext.Orders.Add(orderDB);

                var itemsInUnprocessedOrder = sqliteDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == unprocessedOrders.Id);

                decimal totalAmount = unprocessedOrders.TotalAmount;
                if (itemsInUnprocessedOrder != null && itemsInUnprocessedOrder.Any())
                {
                    itemsInUnprocessedOrder.ToList().ForEach(item =>
                    {
                        var invoiceItem = invoice.ItemInvoices.FirstOrDefault(itemInvoice => itemInvoice.ItemCode == item.ItemId);

                        if(invoiceItem != null)
                        {
                            if (invoiceItem.Quantity.HasValue && 
                            invoiceItem.TotalAmout.HasValue &&
                            invoiceItem.UnitPrice.HasValue)
                            {
                                decimal ta = 0;
                                if (item.Quantity <= invoiceItem.Quantity.Value)
                                {
                                    sqliteDbContext.ItemsInUnprocessedOrder.Remove(item);
                                    ta = item.Quantity * invoiceItem.UnitPrice.Value;
                                }
                                else if (item.Quantity > invoiceItem.Quantity.Value)
                                {
                                    item.Quantity -= invoiceItem.Quantity.Value;
                                    sqliteDbContext.ItemsInUnprocessedOrder.Update(item);
                                    ta = invoiceItem.TotalAmout.Value;
                                }
                                sqliteDbContext.SaveChanges();
                                totalAmount -= ta;
                            }
                        }
                    });
                }
                if(totalAmount > 0)
                {
                    unprocessedOrders.TotalAmount = totalAmount;
                    sqliteDbContext.UnprocessedOrders.Update(unprocessedOrders);
                }
                else
                {
                    sqliteDbContext.UnprocessedOrders.Remove(unprocessedOrders);
                }
                sqliteDbContext.SaveChanges();

                //if (totalAmount > 0)
                //{
                //        if(paySaleViewModel.SaleViewModel.CurrentOrder != null)
                //        {
                //            paySaleViewModel.SaleViewModel.CurrentOrder.Items = items;
                //            paySaleViewModel.SaleViewModel.CurrentOrder.TableId = tableId;
                //            paySaleViewModel.SaleViewModel.CurrentOrder.CashierName = paySaleViewModel.SaleViewModel.LoggedCashier.Name;
                //            paySaleViewModel.SaleViewModel.CurrentOrder.Cashier = paySaleViewModel.SaleViewModel.LoggedCashier;

                //            paySaleViewModel.SaleViewModel.TableOverviewViewModel.SetOrder(paySaleViewModel.SaleViewModel.CurrentOrder);
                //        }
                //}
            }
        }
        private async Task CreateDriverInvoice(Driver driver, InvoiceDB invoiceDB)
        {
            try
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var driverDB = await sqliteDbContext.Drivers.FindAsync(driver.Id);

                if (driverDB != null)
                {
                    DriverInvoiceDB driverInvoiceDB = new DriverInvoiceDB()
                    {
                        DriverId = driverDB.Id,
                        InvoiceId = invoiceDB.Id,
                        IsporukaId = null
                    };

                    sqliteDbContext.DriverInvoices.Add(driverInvoiceDB);
                    sqliteDbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Error("PayCommand -> CreateDriverInvoice -> Greska prilikom kreiranja tabele DriverInvoiceDB -", ex);
            }
        }
        private async Task UpdateInvode(InvoiceDB invoiceDB, decimal total, ResponseJson responseJson)
        {
            try
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                invoiceDB.SdcDateTime = Convert.ToDateTime(responseJson.DateTime);
                invoiceDB.TotalAmount = total;
                invoiceDB.InvoiceCounter = responseJson.TotalInvoiceNumber;
                invoiceDB.InvoiceNumberResult = responseJson.InvoiceNumber;

                sqliteDbContext.Invoices.Update(invoiceDB);
                await sqliteDbContext.SaveChangesAsync();

                if (responseJson.TaxItems != null &&
                    responseJson.TaxItems.Any())
                {
                    responseJson.TaxItems.ToList().ForEach(taxItem =>
                    {
                        TaxItemInvoiceDB taxItemInvoiceDB = new TaxItemInvoiceDB()
                        {
                            Amount = taxItem.Amount,
                            CategoryName = taxItem.CategoryName,
                            CategoryType = (int)taxItem.CategoryType.Value,
                            Label = taxItem.Label,
                            Rate = taxItem.Rate,
                            InvoiceId = invoiceDB.Id
                        };

                        sqliteDbContext.TaxItemInvoices.Add(taxItemInvoiceDB);
                    });
                }
                sqliteDbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Greška prilikom ažuriranja računa!", 
                    "Greška", 
                    MessageBoxButton.OK);
                Log.Error($"PayCommand - UpdateInvode - Greska prilikom update Invoice {invoiceDB.Id}", ex);
                throw;
            }
        }
        private async Task<InvoiceDB> InsertInvoiceInDB(InvoceRequestFileSystemWatcher invoiceRequset,
            List<ItemFileSystemWatcher> items,
            PaySaleViewModel paySaleViewModel,
            DateTime dateTimeOfIssue,
            string? porudzbenica)
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            invoiceRequset.Items = items;

            InvoiceDB invoiceDB = new InvoiceDB()
            {
                Id = Guid.NewGuid().ToString(),
                Porudzbenica = porudzbenica,
                DateAndTimeOfIssue = dateTimeOfIssue,
                Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Id,
                InvoiceType = (int)invoiceRequset.InvoiceType,
                TransactionType = (int)invoiceRequset.TransactionType,
                //SdcDateTime = Convert.ToDateTime(responseJson.DateTime),
                //TotalAmount = total,
                //InvoiceCounter = responseJson.TotalInvoiceNumber,
                //InvoiceNumberResult = responseJson.InvoiceNumber,
                BuyerId = invoiceRequset.BuyerId,
                BuyerName = invoiceRequset.BuyerName,
                BuyerAddress = invoiceRequset.BuyerAddress,
            };

            sqliteDbContext.Add(invoiceDB);
            sqliteDbContext.SaveChanges();

            int itemInvoiceId = 0;
            paySaleViewModel.ItemsInvoice.ToList().ForEach(item =>
            {
                ItemDB? itemDB = sqliteDbContext.Items.Find(item.Item.Id);
                if (itemDB != null)
                {
                    if (itemDB.IdNorm != null)
                    {
                        var norms = sqliteDbContext.ItemsInNorm.Where(norm => norm.IdNorm == itemDB.IdNorm);

                        if (norms != null &&
                            norms.Any())
                        {
                            norms.ForEachAsync(norm =>
                            {
                                var normItem = sqliteDbContext.Items.Find(norm.IdItem);
                                if (normItem != null)
                                {
                                    if (normItem.IdNorm != null)
                                    {
                                        var norms2 = sqliteDbContext.ItemsInNorm.Where(norm => norm.IdNorm == normItem.IdNorm);

                                        if (norms2 != null &&
                                            norms2.Any())
                                        {
                                            norms2.ForEachAsync(norm2 =>
                                            {
                                                var normItem2 = sqliteDbContext.Items.Find(norm2.IdItem);
                                                if (normItem2 != null)
                                                {
                                                    if (normItem2.IdNorm != null)
                                                    {
                                                        var norms3 = sqliteDbContext.ItemsInNorm.Where(norm => norm.IdNorm == normItem2.IdNorm);
                                                        if (norms3 != null &&
                                                            norms3.Any())
                                                        {
                                                            norms3.ForEachAsync(norm3 =>
                                                            {
                                                                var normItem3 = sqliteDbContext.Items.Find(norm3.IdItem);
                                                                if (normItem3 != null)
                                                                {
                                                                    decimal unitPrice = normItem3.InputUnitPrice != null && normItem3.InputUnitPrice.HasValue ?
                                                                    normItem3.InputUnitPrice.Value : 0;

                                                                    var itemInvoice = new ItemInvoiceDB()
                                                                    {
                                                                        Id = itemInvoiceId++,
                                                                        Quantity = Decimal.Round(item.Quantity * norm.Quantity * norm2.Quantity * norm3.Quantity, 3),
                                                                        TotalAmout = Decimal.Round(Decimal.Round(item.Quantity * norm.Quantity * norm2.Quantity * norm3.Quantity, 3) * unitPrice, 2),
                                                                        Label = normItem3.Label,
                                                                        Name = normItem3.Name,
                                                                        UnitPrice = unitPrice,
                                                                        ItemCode = normItem3.Id,
                                                                        OriginalUnitPrice = unitPrice,
                                                                        InvoiceId = invoiceDB.Id,
                                                                        IsSirovina = 1,
                                                                        InputUnitPrice = unitPrice
                                                                        //Item = itemDB
                                                                    };
                                                                    sqliteDbContext.Add(itemInvoice);
                                                                }
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        decimal unitPrice = normItem2.InputUnitPrice != null && normItem2.InputUnitPrice.HasValue ?
                                                        normItem2.InputUnitPrice.Value : 0;

                                                        var itemInvoice = new ItemInvoiceDB()
                                                        {
                                                            Id = itemInvoiceId++,
                                                            Quantity = Decimal.Round(item.Quantity * norm.Quantity * norm2.Quantity, 3),
                                                            TotalAmout = Decimal.Round(Decimal.Round(item.Quantity * norm.Quantity * norm2.Quantity, 3) * unitPrice, 2),
                                                            Label = normItem2.Label,
                                                            Name = normItem2.Name,
                                                            UnitPrice = unitPrice,
                                                            ItemCode = normItem2.Id,
                                                            OriginalUnitPrice = unitPrice,
                                                            InvoiceId = invoiceDB.Id,
                                                            IsSirovina = 1,
                                                            InputUnitPrice = unitPrice
                                                            //Item = itemDB
                                                        };
                                                        sqliteDbContext.Add(itemInvoice);
                                                    }
                                                }
                                            });
                                        }
                                        else
                                        {
                                            decimal unitPrice = normItem.InputUnitPrice != null && normItem.InputUnitPrice.HasValue ?
                                            normItem.InputUnitPrice.Value : 0;

                                            var itemInvoice = new ItemInvoiceDB()
                                            {
                                                Id = itemInvoiceId++,
                                                Quantity = Decimal.Round(item.Quantity * norm.Quantity, 3),
                                                TotalAmout = Decimal.Round(Decimal.Round(item.Quantity * norm.Quantity, 3) * unitPrice, 2),
                                                Label = normItem.Label,
                                                Name = normItem.Name,
                                                UnitPrice = unitPrice,
                                                ItemCode = normItem.Id,
                                                OriginalUnitPrice = unitPrice,
                                                InvoiceId = invoiceDB.Id,
                                                IsSirovina = 1,
                                                InputUnitPrice = unitPrice
                                                //Item = itemDB
                                            };
                                            sqliteDbContext.Add(itemInvoice);
                                        }
                                    }
                                    else
                                    {
                                        decimal unitPrice = normItem.InputUnitPrice != null && normItem.InputUnitPrice.HasValue ?
                                        normItem.InputUnitPrice.Value : 0;

                                        var itemInvoice = new ItemInvoiceDB()
                                        {
                                            Id = itemInvoiceId++,
                                            Quantity = Decimal.Round(item.Quantity * norm.Quantity, 3),
                                            TotalAmout = Decimal.Round(Decimal.Round(item.Quantity * norm.Quantity, 3) * unitPrice, 2),
                                            Label = normItem.Label,
                                            Name = normItem.Name,
                                            UnitPrice = unitPrice,
                                            ItemCode = normItem.Id,
                                            OriginalUnitPrice = unitPrice,
                                            InvoiceId = invoiceDB.Id,
                                            IsSirovina = 1,
                                            InputUnitPrice = unitPrice
                                            //Item = itemDB
                                        };
                                        sqliteDbContext.Add(itemInvoice);
                                    }
                                }
                            });
                        }
                    }

                    var itemInvoice = new ItemInvoiceDB()
                    {
                        Id = itemInvoiceId++,
                        Quantity = Decimal.Round(item.Quantity, 3),
                        TotalAmout = Decimal.Round(Decimal.Round(item.Quantity, 3) * item.Item.SellingUnitPrice, 2),
                        Label = item.Item.Label,
                        Name = item.Item.Name,
                        UnitPrice = item.Item.SellingUnitPrice,
                        ItemCode = item.Item.Id,
                        OriginalUnitPrice = itemDB.SellingUnitPrice,
                        InvoiceId = invoiceDB.Id,
                        IsSirovina = 0,
                        InputUnitPrice = itemDB.InputUnitPrice
                        //Item = itemDB
                    };
                    sqliteDbContext.Add(itemInvoice);
                }
            });

            if (invoiceRequset.Payment != null)
            {
                PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                {
                    InvoiceId = invoiceDB.Id,
                    Amout = invoiceRequset.Payment.Amount,
                    PaymentType = invoiceRequset.Payment.PaymentType
                };

                sqliteDbContext.PaymentInvoices.Add(paymentInvoice);
            }
            sqliteDbContext.SaveChanges();

            return invoiceDB;
        }
        private async Task TakingDownNorm(InvoiceDB invoice)
        {
            try
            {
                List<ItemDB> itemsForCondition = new List<ItemDB>();
                using (var dbContext =  new SqliteDbContext())
                {
                    var itemsInInvoice = dbContext.ItemInvoices.Where(item => item.InvoiceId == invoice.Id &&
                    (item.IsSirovina == 0 || item.IsSirovina == null));

                    if (itemsInInvoice != null && itemsInInvoice.Any())
                    {
                        foreach (var item in itemsInInvoice.ToList())
                        {
                            var it = dbContext.Items.FirstOrDefault(i => i.Id == item.ItemCode);
                            if (it != null && item.Quantity.HasValue)
                            {
                                var itemInNorm = dbContext.ItemsInNorm.Where(norm => it.IdNorm == norm.IdNorm);

                                if (itemInNorm != null && itemInNorm.Any())
                                {
                                    foreach (var norm in itemInNorm)
                                    {
                                        var itm = dbContext.Items.FirstOrDefault(i => i.Id == norm.IdItem);

                                        if (itm != null)
                                        {
                                            if (itm.IdNorm == null)
                                            {
                                                itm.TotalQuantity -= item.Quantity.Value * norm.Quantity;
                                            }
                                            else
                                            {
                                                var itemInNorm2 = dbContext.ItemsInNorm.Where(norm => itm.IdNorm == norm.IdNorm);
                                                if (itemInNorm2.Any())
                                                {
                                                    foreach (var norm2 in itemInNorm2.ToList())
                                                    {
                                                        var itm2 = dbContext.Items.FirstOrDefault(i => i.Id == norm2.IdItem);

                                                        if (itm2 != null)
                                                        {
                                                            if (itm2.IdNorm == null)
                                                            {
                                                                itm2.TotalQuantity -= item.Quantity.Value * norm.Quantity * norm2.Quantity;
                                                            }
                                                            else
                                                            {
                                                                var itemInNorm3 = dbContext.ItemsInNorm.Where(norm => itm2.IdNorm == norm2.IdNorm);
                                                                if (itemInNorm3.Any())
                                                                {
                                                                    foreach (var norm3 in itemInNorm3)
                                                                    {
                                                                        var itm3 = dbContext.Items.FirstOrDefault(i => i.Id == norm3.IdItem);

                                                                        if (itm3 != null)
                                                                        {
                                                                            if (itm3.IdNorm == null)
                                                                            {
                                                                                itm3.TotalQuantity -= item.Quantity.Value * norm.Quantity * norm2.Quantity * norm3.Quantity;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    itm2.TotalQuantity -= item.Quantity.Value * norm.Quantity * norm2.Quantity;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    itm.TotalQuantity -= item.Quantity.Value * norm.Quantity;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    it.TotalQuantity -= item.Quantity.Value;
                                }
                                dbContext.Items.Update(it);
                            }
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PayCommand -> TakingDownNorm -> Greska prilikom skidanja normativa za racun {invoice.Id} - {invoice.InvoiceNumberResult}: ", ex);
            }
        }
        private async void SaveToDB(InvoceRequestFileSystemWatcher invoiceRequset,
            List<ItemFileSystemWatcher> items,
            decimal total,
            PaySaleViewModel paySaleViewModel,
            int tableId,
            ObservableCollection<ItemInvoice> itemsInvoice,
            ResponseJson responseJson,
            InvoiceDB invoiceDB,
            Driver? driver)
        {
            await UpdateInvode(invoiceDB, total, responseJson);
            if(driver != null)
            {
                await CreateDriverInvoice(driver, invoiceDB);
            }
            await TakingDownNorm(invoiceDB);
            //await TakingDownOrder(invoiceDB, paySaleViewModel, tableId, itemsInvoice);
        }
        private async void CreatePonuda(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total,
            List<ItemFileSystemWatcher> items)
        {
            try
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var otpremnice = sqliteDbContext.Invoices.Where(invoice => invoice.InvoiceType == 5 &&
                invoice.TransactionType == 0);

                int otpremnicaIndex = 1;

                if (otpremnice != null &&
                    otpremnice.Any())
                {
                    otpremnicaIndex = otpremnice.Count() + 1;
                }

                DateTime dateTimeOfIssue = DateTime.Now;

                InvoiceDB invoiceDB = new InvoiceDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Id,
                    InvoiceType = 5,
                    TransactionType = 0,
                    SdcDateTime = dateTimeOfIssue,
                    TotalAmount = total,
                    InvoiceNumberResult = $"Ponuda-{otpremnicaIndex}",
                    DateAndTimeOfIssue = dateTimeOfIssue,
                    BuyerId = invoiceRequset.BuyerId,
                    BuyerName = invoiceRequset.BuyerName,
                    BuyerAddress = invoiceRequset.BuyerAddress,
                };
                sqliteDbContext.Add(invoiceDB);

                sqliteDbContext.SaveChanges();

                List<UniversalEsir_Common.Models.Invoice.Item> itemsOtpremnica = new List<UniversalEsir_Common.Models.Invoice.Item>();

                int itemInvoiceId = 0;
                items.ForEach(item =>
                {
                    ItemDB? itemDB = sqliteDbContext.Items.Find(item.Id);
                    if (itemDB != null)
                    {
                        ItemInvoiceDB itemInvoiceDB = new ItemInvoiceDB()
                        {
                            Id = itemInvoiceId++,
                            Quantity = item.Quantity,
                            TotalAmout = item.TotalAmount,
                            Label = item.Label,
                            Name = item.Name,
                            UnitPrice = item.UnitPrice,
                            ItemCode = item.Id,
                            InvoiceId = invoiceDB.Id
                            //Item = itemDB
                        };

                        sqliteDbContext.Add(itemInvoiceDB);

                        itemsOtpremnica.Add(new UniversalEsir_Common.Models.Invoice.Item()
                        {
                            Name = item.Name,
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Quantity,
                            TotalAmount = item.TotalAmount,
                            Jm = item.Jm,
                        });
                    }
                });
                sqliteDbContext.SaveChanges();

                PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                {
                    InvoiceId = invoiceDB.Id,
                    Amout = paySaleViewModel.Payment.Amount,
                    PaymentType = paySaleViewModel.Payment.PaymentType
                };

                sqliteDbContext.PaymentInvoices.Add(paymentInvoice);

                sqliteDbContext.SaveChanges();

                Otpremnica otpremnica = new Otpremnica()
                {
                    SdcDateTime = invoiceDB.SdcDateTime.Value,
                    BuyerId = invoiceDB.BuyerId,
                    BuyerName = invoiceDB.BuyerName,
                    BuyerAddress = invoiceDB.BuyerAddress,
                    InvoiceNumberResult = invoiceDB.InvoiceNumberResult,
                    TotalAmount = invoiceDB.TotalAmount.Value,
                };
                otpremnica.Items = itemsOtpremnica;

                //var firma = sqliteDbContext.Firmas.FirstOrDefault();
                //if (firma != null )
                //{
                //    otpremnica.Tin = firma.Pib;
                //    otpremnica.BusinessName = firma.Name;
                //    otpremnica.Address = firma.AddressPP;
                //    otpremnica.MB = firma.MB;
                //    otpremnica.BankAccount = firma.BankAcc;
                //}

                PrinterManager.Instance.PrintPonuda(otpremnica);
                paySaleViewModel.SaleViewModel.Reset();
            }
            catch (Exception ex)
            {
                Log.Error("PayCommand -> BlackOtpremnica -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private async void BlackOtpremnica(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total,
            List<ItemFileSystemWatcher> items,
            string? porudzbenica,
            Driver? driver)
        {
            try
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var otpremnice = sqliteDbContext.Invoices.Where(invoice => invoice.InvoiceType == 5 &&
                invoice.TransactionType == 0);

                int otpremnicaIndex = 1;

                if (otpremnice != null &&
                    otpremnice.Any())
                {
                    otpremnicaIndex = otpremnice.Count() + 1;
                }

                DateTime dateTimeOfIssue = DateTime.Now;

                InvoiceDB invoiceDB = new InvoiceDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Id,
                    InvoiceType = 5,
                    TransactionType = 0,
                    SdcDateTime = dateTimeOfIssue,
                    TotalAmount = total,
                    InvoiceNumberResult = $"Otpremnica-{otpremnicaIndex}",
                    Porudzbenica = porudzbenica,
                    DateAndTimeOfIssue = dateTimeOfIssue,
                    BuyerId = invoiceRequset.BuyerId,
                    BuyerName = invoiceRequset.BuyerName,
                    BuyerAddress = invoiceRequset.BuyerAddress,
                };
                sqliteDbContext.Add(invoiceDB);

                sqliteDbContext.SaveChanges();

                List<UniversalEsir_Common.Models.Invoice.Item> itemsOtpremnica = new List<UniversalEsir_Common.Models.Invoice.Item>();

                int itemInvoiceId = 0;
                items.ForEach(item =>
                {
                    ItemDB? itemDB = sqliteDbContext.Items.Find(item.Id);
                    if (itemDB != null)
                    {
                        ItemInvoiceDB itemInvoiceDB = new ItemInvoiceDB()
                        {
                            Id = itemInvoiceId++,
                            Quantity = item.Quantity,
                            TotalAmout = item.TotalAmount,
                            Label = item.Label,
                            Name = item.Name,
                            UnitPrice = item.UnitPrice,
                            ItemCode = item.Id,
                            InvoiceId = invoiceDB.Id
                            //Item = itemDB
                        };

                        sqliteDbContext.Add(itemInvoiceDB);

                        itemsOtpremnica.Add(new UniversalEsir_Common.Models.Invoice.Item()
                        {
                            Name = item.Name,
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Quantity,
                            TotalAmount = item.TotalAmount,
                            Jm = item.Jm,
                        });
                    }
                });
                sqliteDbContext.SaveChanges();

                
                PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                {
                    InvoiceId = invoiceDB.Id,
                    Amout = paySaleViewModel.Payment.Amount,
                    PaymentType = paySaleViewModel.Payment.PaymentType
                };

                sqliteDbContext.PaymentInvoices.Add(paymentInvoice);

                sqliteDbContext.SaveChanges();

                if (driver != null)
                {
                    await CreateDriverInvoice(driver, invoiceDB);
                }

                Otpremnica otpremnica = new Otpremnica()
                {
                    Porudzbenica = porudzbenica,
                    SdcDateTime = invoiceDB.SdcDateTime.Value,
                    BuyerId = invoiceDB.BuyerId,
                    BuyerName = invoiceDB.BuyerName,
                    BuyerAddress = invoiceDB.BuyerAddress,
                    InvoiceNumberResult = invoiceDB.InvoiceNumberResult,
                    TotalAmount = invoiceDB.TotalAmount.Value,
                };
                otpremnica.Items = itemsOtpremnica;

                //var firma = sqliteDbContext.Firmas.FirstOrDefault();
                //if (firma != null )
                //{
                //    otpremnica.Tin = firma.Pib;
                //    otpremnica.BusinessName = firma.Name;
                //    otpremnica.Address = firma.AddressPP;
                //    otpremnica.MB = firma.MB;
                //    otpremnica.BankAccount = firma.BankAcc;
                //}

                PrinterManager.Instance.PrintVirman(otpremnica);
                paySaleViewModel.SaleViewModel.Reset();
            }
            catch (Exception ex)
            {
                Log.Error("PayCommand -> BlackOtpremnica -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void Black(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total,
            List<ItemFileSystemWatcher> items)
        {
            Task.Run(() =>
            {
                PrinterManager.Instance.PrintJournal(invoiceRequset);
            });

            ObservableCollection<ItemInvoice> itemsInvoice = new ObservableCollection<ItemInvoice>(paySaleViewModel.SaleViewModel.ItemsInvoice);

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

            InvoiceDB invoiceDB = new InvoiceDB()
            {
                Id = Guid.NewGuid().ToString(),
                Cashier = paySaleViewModel.SaleViewModel.LoggedCashier.Id,
                InvoiceType = 0,
                TransactionType = 0,
                SdcDateTime = DateTime.Now,
                TotalAmount = total,
                IsSend = 0,
                ClanId = paySaleViewModel.SaleViewModel.CurrentClan.Id,
            };
            sqliteDbContext.Add(invoiceDB);

            sqliteDbContext.SaveChanges();

            if (invoiceRequset.Payment != null)
            {
                PaymentInvoiceDB paymentInvoice = new PaymentInvoiceDB()
                {
                    InvoiceId = invoiceDB.Id,
                    Amout = invoiceRequset.Payment.Amount,
                    PaymentType = invoiceRequset.Payment.PaymentType
                };

                sqliteDbContext.PaymentInvoices.Add(paymentInvoice);
            }
            sqliteDbContext.SaveChanges();

            var neposlatiRacuni = sqliteDbContext.Invoices.Include(i => i.PaymentInvoices).Include(i => i.ItemInvoices).Where(i => i.IsSend == 0 &&
            i.Id != invoiceDB.Id);
            if (neposlatiRacuni != null &&
                neposlatiRacuni.Any())
            {
                try
                {
                    foreach (var racun in neposlatiRacuni)
                    {
                        var placenoStaro = racun.PaymentInvoices.FirstOrDefault(p => p.PaymentType == PaymentTypeEnumeration.Crta);

                        var kotizacijaStaro = racun.ItemInvoices.FirstOrDefault(i => i.Name.ToLower().Contains("kotizacija"));

                        if (kotizacijaStaro != null)
                        {
                            ZaduzenjeRequest zaduzenjeRequestStaro = new ZaduzenjeRequest()
                            {
                                Date = DateTime.Now,
                                TotalAmount = racun.TotalAmount.Value,
                                Placeno = placenoStaro == null ? 0 : placenoStaro.Amout.Value,
                                Type = (int)ZaduzenjeEnumeration.Kotizacije,
                                UserId = racun.ClanId,
                                Opis = kotizacijaStaro != null ? kotizacijaStaro.Name : null,
                                Otpis = 0
                            };

                            if (!sportSchedulerAPI_Manager.PostZaduzenjeAsync(zaduzenjeRequestStaro).Result)
                            {
                                MessageBox.Show("GREŠKA PRILIKOM slanja zaduzenja za kotizaciju na server!", "GREŠKA", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                racun.IsSend = 1;
                                sqliteDbContext.Invoices.Update(racun);
                                sqliteDbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            var prodavnicaStaro = sqliteDbContext.Items.Join(sqliteDbContext.ItemGroups,
                                item => item.IdItemGroup,
                                group => group.Id,
                                (item, group) => new { Item = item, Group = group }).
                                Where(i => i.Group.Name.ToLower().Contains("prodavnica"));

                            RacunRequest racunRequestStaro = new RacunRequest()
                            {
                                UserId = racun.ClanId,
                                Date = DateTime.Now,
                                TotalAmount = racun.TotalAmount.Value,
                                Placeno = placenoStaro == null ? 0 : placenoStaro.Amout.Value,
                                Items = new List<ItemRacunRequest>(),
                                Type = (int)RacunEnumeration.Kafic
                            };

                            RacunRequest prodavnicaRequestStaro = new RacunRequest()
                            {
                                UserId = racun.ClanId,
                                Date = DateTime.Now,
                                TotalAmount = racun.TotalAmount.Value,
                                Placeno = placenoStaro == null ? 0 : placenoStaro.Amout.Value,
                                Items = new List<ItemRacunRequest>(),
                                Type = (int)RacunEnumeration.Prodavnica
                            };

                            foreach (var item in racun.ItemInvoices)
                            {
                                if (prodavnicaStaro.FirstOrDefault(i => i.Item.Id == item.ItemCode) != null)
                                {
                                    racunRequestStaro.Items.Add(new ItemRacunRequest()
                                    {
                                        ItemsId = item.ItemCode,
                                        Name = item.Name,
                                        Quantity = item.Quantity.Value,
                                        TotalAmount = item.TotalAmout.Value,
                                        UnitPrice = item.UnitPrice.Value
                                    });
                                }
                                else
                                {
                                    prodavnicaRequestStaro.Items.Add(new ItemRacunRequest()
                                    {
                                        ItemsId = item.ItemCode,
                                        Name = item.Name,
                                        Quantity = item.Quantity.Value,
                                        TotalAmount = item.TotalAmout.Value,
                                        UnitPrice = item.UnitPrice.Value
                                    });
                                }
                            }
                            try
                            {
                                if (racunRequestStaro.Items.Any())
                                {
                                    if (!sportSchedulerAPI_Manager.PostRacunAsync(racunRequestStaro).Result)
                                    {
                                        MessageBox.Show("GREŠKA PRILIKOM slanja racuna za kafic na server!", "GREŠKA", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        racun.IsSend = 1;
                                        sqliteDbContext.Invoices.Update(racun);
                                        sqliteDbContext.SaveChanges();
                                    }
                                }
                                if (prodavnicaRequestStaro.Items.Any())
                                {
                                    racun.IsSend = 0;
                                    if (!sportSchedulerAPI_Manager.PostRacunAsync(prodavnicaRequestStaro).Result)
                                    {
                                        MessageBox.Show("GREŠKA PRILIKOM slanja racuna za prodavnicu na server!", "GREŠKA", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        racun.IsSend = 1;
                                    }

                                    sqliteDbContext.Invoices.Update(racun);
                                    sqliteDbContext.SaveChanges();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("PayCommand -> Black -> GREŠKA PRILIKOM slanja STAROG racuna na server", ex);
                                MessageBox.Show("GREŠKA PRILIKOM slanja racuna na server!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("PayCommand -> Black -> Desila se greska: ", ex);
                }
            }

            int itemInvoiceId = 0;
            List<ItemInvoiceDB> itemsInvoiceDB = new List<ItemInvoiceDB>();

            Payment? placeno = null;

            if (invoiceRequset.Payment != null &&
                invoiceRequset.Payment.PaymentType == PaymentTypeEnumeration.Cash)
            {
                placeno = invoiceRequset.Payment;
            }

            var kotizacija = items.FirstOrDefault(i => i.Name.ToLower().Contains("kotizacija"));

            if (kotizacija != null)
            {
                ZaduzenjeRequest zaduzenjeRequest = new ZaduzenjeRequest()
                {
                    Date = DateTime.Now,
                    TotalAmount = total,
                    Placeno = placeno == null ? 0 : placeno.Amount,
                    Type = (int)ZaduzenjeEnumeration.Kotizacije,
                    UserId = paySaleViewModel.SaleViewModel.CurrentClan.Id,
                    Opis = kotizacija != null ? kotizacija.Name : null,
                    Otpis = 0
                };

                foreach (var item in items)
                {
                    ItemDB? itemDB = sqliteDbContext.Items.Find(item.Id);
                    if (itemDB != null)
                    {
                        itemsInvoiceDB.Add(new ItemInvoiceDB()
                        {
                            Id = itemInvoiceId++,
                            Quantity = item.Quantity,
                            TotalAmout = item.TotalAmount,
                            Label = item.Label,
                            Name = item.Name,
                            UnitPrice = item.UnitPrice,
                            ItemCode = item.Id
                            //Item = itemDB
                        });

                        itemsInvoiceDB.ForEach(itemInvoice =>
                        {
                            itemInvoice.InvoiceId = invoiceDB.Id;
                            //itemInvoice.Invoice = invoice;

                            sqliteDbContext.Add(itemInvoice);
                        });
                        sqliteDbContext.SaveChanges();
                    }
                }

                if (!sportSchedulerAPI_Manager.PostZaduzenjeAsync(zaduzenjeRequest).Result)
                {
                    MessageBox.Show("GREŠKA PRILIKOM slanja zaduzenja za kotizaciju na server!", "GREŠKA", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    invoiceDB.IsSend = 1;
                    sqliteDbContext.Invoices.Update(invoiceDB);
                    sqliteDbContext.SaveChanges();
                }
            }
            else
            {
                var prodavnica = sqliteDbContext.Items.Join(sqliteDbContext.ItemGroups,
                    item => item.IdItemGroup,
                    group => group.Id,
                    (item, group) => new { Item = item, Group = group }).
                    Where(i => i.Group.Name.ToLower().Contains("prodavnica"));

                RacunRequest racunRequest = new RacunRequest()
                {
                    UserId = paySaleViewModel.CurrentClan.Id,
                    Date = DateTime.Now,
                    TotalAmount = total,
                    Placeno = placeno == null ? 0 : placeno.Amount,
                    Items = new List<ItemRacunRequest>(),
                    Type = (int)RacunEnumeration.Kafic
                };

                RacunRequest prodavnicaRequest = new RacunRequest()
                {
                    UserId = paySaleViewModel.CurrentClan.Id,
                    Date = DateTime.Now,
                    TotalAmount = total,
                    Placeno = placeno == null ? 0 : placeno.Amount,
                    Items = new List<ItemRacunRequest>(),
                    Type = (int)RacunEnumeration.Prodavnica
                };

                foreach (var item in items)
                {
                    ItemDB? itemDB = sqliteDbContext.Items.Find(item.Id);
                    if (itemDB != null)
                    {
                        itemsInvoiceDB.Add(new ItemInvoiceDB()
                        {
                            Id = itemInvoiceId++,
                            Quantity = item.Quantity,
                            TotalAmout = item.TotalAmount,
                            Label = item.Label,
                            Name = item.Name,
                            UnitPrice = item.UnitPrice,
                            ItemCode = item.Id
                            //Item = itemDB
                        });

                        if (prodavnica.FirstOrDefault(i => i.Item.Id == item.Id) != null)
                        {
                            prodavnicaRequest.Items.Add(new ItemRacunRequest()
                            {
                                ItemsId = item.Id,
                                Name = item.Name,
                                Quantity = item.Quantity,
                                TotalAmount = item.TotalAmount,
                                UnitPrice = item.UnitPrice
                            });
                        }
                        else
                        {
                            racunRequest.Items.Add(new ItemRacunRequest()
                            {
                                ItemsId = item.Id,
                                Name = item.Name,
                                Quantity = item.Quantity,
                                TotalAmount = item.TotalAmount,
                                UnitPrice = item.UnitPrice
                            });
                        }
                    }
                }

                itemsInvoiceDB.ForEach(itemInvoice =>
                {
                    itemInvoice.InvoiceId = invoiceDB.Id;
                    //itemInvoice.Invoice = invoice;

                    sqliteDbContext.Add(itemInvoice);
                });
                sqliteDbContext.SaveChanges();

                TakingDownNorm(invoiceDB);
                TakingDownOrder(invoiceDB, paySaleViewModel, paySaleViewModel.SaleViewModel.TableId, itemsInvoice);
                try
                {
                    if (racunRequest.Items.Any())
                    {
                        if (!sportSchedulerAPI_Manager.PostRacunAsync(racunRequest).Result)
                        {
                            MessageBox.Show("GREŠKA PRILIKOM slanja racuna za kafic na server!", "GREŠKA", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            invoiceDB.IsSend = 1;
                            sqliteDbContext.Invoices.Update(invoiceDB);
                            sqliteDbContext.SaveChanges();
                        }
                    }

                    if (prodavnicaRequest.Items.Any())
                    {
                        invoiceDB.IsSend = 0;
                        if (!sportSchedulerAPI_Manager.PostRacunAsync(prodavnicaRequest).Result)
                        {
                            MessageBox.Show("GREŠKA PRILIKOM slanja racuna za prodavnicu na server!", "GREŠKA", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            invoiceDB.IsSend = 1;
                        }

                        sqliteDbContext.Invoices.Update(invoiceDB);
                        sqliteDbContext.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("PayCommand -> Black -> GREŠKA PRILIKOM slanja racuna na server", ex);
                    MessageBox.Show("GREŠKA PRILIKOM slanja racuna na server!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            paySaleViewModel.SaleViewModel.Reset();

            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                paySaleViewModel.SaleViewModel.LoggedCashier,
                paySaleViewModel.SaleViewModel);
            paySaleViewModel.SaleViewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
        }
        private async void Normal(InvoceRequestFileSystemWatcher invoiceRequset,
            PaySaleViewModel paySaleViewModel,
            decimal total, 
            List<ItemFileSystemWatcher> items,
            decimal popust)
        {
            string json = JsonConvert.SerializeObject(invoiceRequset);

            string? inDirectory = SettingsManager.Instance.GetInDirectory();

            if (string.IsNullOrEmpty(inDirectory))
            {
                MessageBox.Show("Putanja do ulaznog foldera nije setovana! Račun ne moze da se fiskalizuje.",
                    "Ulazni folder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            else
            {
                DateTime dateTimeOfIssue = DateTime.Now;

                string invoiceName = $"Invoice_{dateTimeOfIssue.ToString("dd-MM-yyyy HH-mm-ss")}.json";
                string jsonPath = System.IO.Path.Combine(inDirectory, invoiceName);

                File.WriteAllText(jsonPath, json);

                int tableId = paySaleViewModel.SaleViewModel.TableId;
                ObservableCollection<ItemInvoice> itemsInvoice = new ObservableCollection<ItemInvoice>(paySaleViewModel.SaleViewModel.ItemsInvoice);

                InvoiceDB invoiceDB = await InsertInvoiceInDB(invoiceRequset, 
                    items, 
                    paySaleViewModel,
                    dateTimeOfIssue,
                    paySaleViewModel.Porudzbenica);


                _ = Task.Run(() =>
                {
                    string? outDirectory = SettingsManager.Instance.GetOutDirectory();

                    if (!string.IsNullOrEmpty(outDirectory))
                    {
                        string jsonOutPath = System.IO.Path.Combine(outDirectory, invoiceName);

                        _timer = DateTime.Now;
                        while (IsFileLocked(jsonOutPath)) ;

                        try
                        {
                            using (StreamReader r = new StreamReader(jsonOutPath))
                            {
                                string response = r.ReadToEnd();

                                ResponseJson? responseJson = JsonConvert.DeserializeObject<ResponseJson>(response);

                                if (responseJson != null)
                                {
                                    if (responseJson.Message.Contains("Uspešna fiskalizacija"))
                                    {
                                        try
                                        {
                                            SaveToDB(invoiceRequset,
                                                items,
                                                total,
                                                paySaleViewModel,
                                                tableId,
                                                itemsInvoice,
                                                responseJson,
                                                invoiceDB,
                                                paySaleViewModel.CurrentDriver);
                                        }
                                        catch
                                        {
                                            MessageBox.Show("GREŠKA PRILIKOM UPISA U BAZU!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("PROVERITE ESIR I LPFR!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("PayCommand -> FinisedSale -> ", ex);
                            MessageBox.Show("GREŠKA U PROVERI IZLAZNOG FAJLA!\nPROVERITE DA LI JE ESIR POKRENUT", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                });

                paySaleViewModel.SaleViewModel.SendToDisplay("* * * HVALA * * *");
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    paySaleViewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    paySaleViewModel.SaleViewModel.Reset();
                }
            }
        }
        private bool IsFileLocked(string file)
        {
            //check that problem is not in destination file
            if (File.Exists(file))
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (Exception ex2)
                {
                    //_log.WriteLog(ex2, "Error in checking whether file is locked " + file);
                    int errorCode = Marshal.GetHRForException(ex2) & ((1 << 16) - 1);
                    if ((ex2 is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
                    {
                        return true;
                    }
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
            else
            {
                if (DateTime.Now.Subtract(_timer).TotalSeconds > 60)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
