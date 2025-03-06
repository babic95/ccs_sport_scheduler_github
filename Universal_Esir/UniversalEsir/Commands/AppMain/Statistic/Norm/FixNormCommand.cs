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
using System.Windows;
using Microsoft.EntityFrameworkCore;
using UniversalEsir_Logging;
using SQLitePCL;
using UniversalEsir.Models.Sale;
using UniversalEsir_eFaktura.Models;

namespace UniversalEsir.Commands.AppMain.Statistic.Norm
{
    public class FixNormCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NormViewModel _currentViewModel;

        public FixNormCommand(NormViewModel currentViewModel)
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
                if (_currentViewModel.FromDate == null)
                {
                    _currentViewModel.FromDate = DateTime.Now;
                }
                if (_currentViewModel.ToDate == null)
                {
                    _currentViewModel.ToDate = DateTime.Now;
                }

                if (_currentViewModel.FromDate > _currentViewModel.ToDate)
                {
                    MessageBox.Show("Početni datum mora biti stariji od krajnjeg!", "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                    return;
                }

                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var invoices = sqliteDbContext.Invoices.Where(invoice => invoice.SdcDateTime >= _currentViewModel.FromDate.Value &&
                    invoice.SdcDateTime <= _currentViewModel.ToDate.Value &&
                    invoice.InvoiceType == 0);

                if (invoices != null &&
                    invoices.Any())
                {
                    invoices.ForEachAsync(invoice =>
                    {
                        if(invoice.Id == "0765d74a-f16f-4962-b97d-f6a67dd93e74" ||
                        invoice.Id == "eaf645f3-9f21-4afa-a4ba-efd0c4dec75e" ||
                        invoice.Id == "101fc810-9717-4dab-95be-5513187b9d49" ||
                        invoice.Id == "821195fe-27d0-44cb-a920-9c9ce00ceceb" ||
                        invoice.Id == "2e1ec876-ffd5-4d51-b5f4-f6d7d06b623b")
                        {
                            int a = 2;
                        }

                        List<ItemInNormForChange> itemInNormForChanges = new List<ItemInNormForChange>();

                        var itemsInInvoiceSirovine = sqliteDbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id &&
                        itemInvoice.IsSirovina == 1);

                        if (itemsInInvoiceSirovine != null &&
                        itemsInInvoiceSirovine.Any())
                        {

                            itemsInInvoiceSirovine.ForEachAsync(itemInInvoice =>
                            {
                                var itemDB = sqliteDbContext.Items.Find(itemInInvoice.ItemCode);

                                if (itemDB != null)
                                {
                                    ReduceNormHasNorm(sqliteDbContext,
                                        invoice,
                                        itemDB,
                                        itemInInvoice,
                                        itemInNormForChanges);
                                }
                            });
                        }
                        else
                        {
                            var itemsInInvoiceNotSirovina = sqliteDbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id);

                            if (itemsInInvoiceNotSirovina != null &&
                                itemsInInvoiceNotSirovina.Any())
                            {
                                itemsInInvoiceNotSirovina.ForEachAsync(itemInvoice =>
                                {
                                    var itemDB = sqliteDbContext.Items.Find(itemInvoice.ItemCode);

                                    if (itemDB != null)
                                    {
                                        if (itemDB.IdNorm != null)
                                        {
                                            var norms2 = sqliteDbContext.ItemsInNorm.Where(itemInNorm => itemInNorm.IdNorm == itemDB.IdNorm);

                                            if (norms2 != null &&
                                            norms2.Any())
                                            {
                                                norms2.ForEachAsync(itemInNorm2 =>
                                                {
                                                    var itemDB2 = sqliteDbContext.Items.Find(itemInNorm2.IdItem);

                                                    if (itemDB2 != null)
                                                    {
                                                        if (itemDB2.IdNorm != null)
                                                        {
                                                            var norms3 = sqliteDbContext.ItemsInNorm.Where(itemInNorm =>
                                                            itemInNorm.IdNorm == itemDB2.IdNorm);

                                                            if (norms3 != null &&
                                                            norms3.Any())
                                                            {
                                                                norms3.ForEachAsync(itemInNorm3 =>
                                                                {
                                                                    var itemDB3 = sqliteDbContext.Items.Find(itemInNorm3.IdItem);

                                                                    if (itemDB3 != null)
                                                                    {
                                                                        ReduceNormNoNorm(sqliteDbContext,
                                                                            invoice,
                                                                            itemDB3,
                                                                            itemInNorm3.Quantity,
                                                                            itemInNormForChanges);
                                                                    }
                                                                });
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ReduceNormNoNorm(sqliteDbContext,
                                                                invoice,
                                                                itemDB2,
                                                                itemInNorm2.Quantity,
                                                                itemInNormForChanges);
                                                        }
                                                    }
                                                });
                                            }
                                        }
                                    }
                                });
                            }
                        }
                        sqliteDbContext.SaveChanges();

                        var itemsInInvoice = sqliteDbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id &&
                        (itemInvoice.IsSirovina == null || itemInvoice.IsSirovina == 0));

                        if (itemsInInvoice != null &&
                        itemsInInvoice.Any())
                        {
                            itemsInInvoice.ForEachAsync(itemInvoice =>
                            {
                                var itemDB = sqliteDbContext.Items.Find(itemInvoice.ItemCode);

                                if (itemDB != null)
                                {
                                    if (itemDB.IdNorm != null)
                                    {
                                        decimal quantity = 0;
                                        if (itemInvoice.Quantity.HasValue)
                                        {
                                            quantity = itemInvoice.Quantity.Value;
                                        }
                                                                                                                    
                                        var norms1 = sqliteDbContext.ItemsInNorm.Where(itemInNorm => itemInNorm.IdNorm == itemDB.IdNorm);

                                        if (norms1 != null &&
                                        norms1.Any())
                                        {
                                            norms1.ForEachAsync(itemInNorm1 =>
                                            {
                                                decimal quantity1 = quantity * itemInNorm1.Quantity;
                                                var itemDB2 = sqliteDbContext.Items.Find(itemInNorm1.IdItem);

                                                if (itemDB2 != null)
                                                {
                                                    if (itemDB2.IdNorm != null)
                                                    {
                                                        var norms2 = sqliteDbContext.ItemsInNorm.Where(itemInNorm =>
                                                        itemInNorm.IdNorm == itemDB2.IdNorm);

                                                        if (norms2 != null &&
                                                        norms2.Any())
                                                        {
                                                            norms2.ForEachAsync(itemInNorm2 =>
                                                            {
                                                                decimal quantity2 = quantity1 * itemInNorm2.Quantity;
                                                                var itemDB3 = sqliteDbContext.Items.Find(itemInNorm2.IdItem);

                                                                if (itemDB3 != null)
                                                                {
                                                                    if (itemDB3.IdNorm != null)
                                                                    {
                                                                        var norms3 = sqliteDbContext.ItemsInNorm.Where(itemInNorm =>
                                                                        itemInNorm.IdNorm == itemDB3.IdNorm);

                                                                        if(norms3 != null &&
                                                                        norms3.Any())
                                                                        {
                                                                            norms3.ForEachAsync(itemInNorm3 =>
                                                                            {
                                                                                decimal quantity3 = quantity2 * itemInNorm3.Quantity;
                                                                                var itemDB4 = sqliteDbContext.Items.Find(itemInNorm3.IdItem);

                                                                                if(itemDB4 != null)
                                                                                {
                                                                                    IncreaseNorm(sqliteDbContext,
                                                                                        invoice,
                                                                                        itemDB4,
                                                                                        quantity3,
                                                                                        itemInNormForChanges);
                                                                                }
                                                                            });
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        IncreaseNorm(sqliteDbContext,
                                                                            invoice,
                                                                            itemDB3,
                                                                            quantity2,
                                                                            itemInNormForChanges);
                                                                    }
                                                                }
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        IncreaseNorm(sqliteDbContext,
                                                            invoice,
                                                            itemDB2,
                                                            quantity1,
                                                            itemInNormForChanges);
                                                    }
                                                }
                                            });
                                        }
                                    }
                                }
                            });
                        }
                    });
                }
                sqliteDbContext.SaveChangesAsync();


                //var invoices = sqliteDbContext.Invoices.Join(sqliteDbContext.ItemInvoices,
                //invoice => invoice.Id,
                //itemInInvoice => itemInInvoice.InvoiceId,
                //(invoice, itemInInvoice) => new { Invoice = invoice, ItemInInvoice = itemInInvoice })
                //.Where(invoice => invoice.Invoice.SdcDateTime >= _currentViewModel.FromDate.Value &&
                //invoice.Invoice.SdcDateTime <= _currentViewModel.ToDate.Value &&
                //(invoice.ItemInInvoice.IsSirovina == null || invoice.ItemInInvoice.IsSirovina == 0))
                //.Join(sqliteDbContext.Items,
                //invoice => invoice.ItemInInvoice.ItemCode,
                //item => item.Id,
                //(invoice, item) => new { Invoice = invoice, Item = item });

                //if (invoices != null &&
                //    invoices.Any())
                //{
                //    invoices.ToList().ForEach(invoice =>
                //    {
                //        if (invoice.Invoice.Invoice.InvoiceType == 0)
                //        {
                //            int lastSalsItemIndex = -1;
                //            if (invoice.Invoice.Invoice.Id == "9b8c0fad-d499-4705-bad3-0a0cc0f9a925")
                //            {
                //                int a = 2;
                //            }
                //            if (invoice.Item.IdNorm != null)
                //            {
                //                var norms = sqliteDbContext.ItemsInNorm.Where(norm => norm.IdNorm == invoice.Item.IdNorm);
                //                if (norms != null &&
                //                    norms.Any())
                //                {
                //                    norms.ToList().ForEach(norm =>
                //                    {
                //                        var itemDB = sqliteDbContext.Items.Find(norm.IdItem);
                //                        if (itemDB != null)
                //                        {
                //                            decimal previousQuantity = norm.Quantity;

                //                            if (itemDB.IdNorm != null)
                //                            {
                //                                var norms2 = sqliteDbContext.ItemsInNorm.Where(n => n.IdNorm == itemDB.IdNorm);
                //                                if (norms2 != null &&
                //                                    norms2.Any())
                //                                {
                //                                    norms2.ToList().ForEach(norm2 =>
                //                                    {
                //                                        var itemDB2 = sqliteDbContext.Items.Find(norm2.IdItem);

                //                                        if (itemDB2 != null)
                //                                        {
                //                                            decimal previousQuantity2 = previousQuantity * norm2.Quantity;
                //                                            if (itemDB2.IdNorm != null)
                //                                            {
                //                                                var norms3 = sqliteDbContext.ItemsInNorm.Where(n => n.IdNorm == itemDB2.IdNorm);
                //                                                if (norms3 != null &&
                //                                                    norms3.Any())
                //                                                {
                //                                                    norms3.ToList().ForEach(norm3 =>
                //                                                    {
                //                                                        var itemDB3 = sqliteDbContext.Items.Find(norm3.IdItem);

                //                                                        if (itemDB3 != null)
                //                                                        {
                //                                                            UpdateNorm(sqliteDbContext,
                //                                                                norm3,
                //                                                                invoice.Invoice.Invoice,
                //                                                                itemDB3,
                //                                                                lastSalsItemIndex,
                //                                                                invoice.Invoice.ItemInInvoice.Id,
                //                                                                previousQuantity2 * norm3.Quantity);
                //                                                        }
                //                                                    });
                //                                                }
                //                                            }
                //                                            else
                //                                            {
                //                                                UpdateNorm(sqliteDbContext,
                //                                                    norm2,
                //                                                    invoice.Invoice.Invoice,
                //                                                    itemDB2,
                //                                                    lastSalsItemIndex,
                //                                                    invoice.Invoice.ItemInInvoice.Id,
                //                                                    previousQuantity2);
                //                                            }
                //                                        }
                //                                    });
                //                                }
                //                            }
                //                            else
                //                            {
                //                                UpdateNorm(sqliteDbContext,
                //                                    norm,
                //                                    invoice.Invoice.Invoice,
                //                                    itemDB,
                //                                    lastSalsItemIndex,
                //                                    invoice.Invoice.ItemInInvoice.Id,
                //                                    previousQuantity);
                //                            }
                //                        }
                //                    });
                //                }
                //            }
                //            lastSalsItemIndex = invoice.Invoice.ItemInInvoice.Id;
                //        }
                //    });
                //}

                MessageBox.Show("Uspešno ste sredili stanje sirovina za zadati period!", "Uspešno",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom sredjivanja normativa!", "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                Log.Error("FixNormCommand -> Greska prilikom sredjivanja normativa -> ", ex);
            }
        }
        private void UpdateNorm(SqliteDbContext sqliteDbContext,
            ItemInNormDB normDB,
            InvoiceDB invoiceDB,
            ItemDB itemDB,
            int lastSalsItemIndex,
            int currentSalsItemIndex,
            decimal quantity)
        {
            if (invoiceDB.Id == "9b8c0fad-d499-4705-bad3-0a0cc0f9a925")
            {
                int a = 2;
            }
            var itemInvoiceDB = sqliteDbContext.ItemInvoices.FirstOrDefault(itemInvoice =>
            itemInvoice.ItemCode == normDB.IdItem && itemInvoice.InvoiceId == invoiceDB.Id &&
            itemInvoice.Id > lastSalsItemIndex && itemInvoice.Id < currentSalsItemIndex);

            if (itemInvoiceDB != null)
            {
                decimal price = itemInvoiceDB.UnitPrice.HasValue ? itemInvoiceDB.UnitPrice.Value : 0;
                decimal oldQuantity = itemInvoiceDB.Quantity.HasValue ? itemInvoiceDB.Quantity.Value : 0;

                if (quantity != oldQuantity)
                {
                    if (invoiceDB.TransactionType == 0)
                    {
                        itemDB.TotalQuantity -= (quantity - oldQuantity);
                    }
                    else
                    {
                        itemDB.TotalQuantity += (quantity - oldQuantity);
                    }

                    itemInvoiceDB.Quantity = quantity;
                    itemInvoiceDB.TotalAmout = quantity * price;
                    sqliteDbContext.Items.Update(itemDB);
                    sqliteDbContext.ItemInvoices.Update(itemInvoiceDB);
                }
            }
            else
            {
                var itemsInvoice = sqliteDbContext.ItemInvoices.Where(itemInvoice =>
                itemInvoice.InvoiceId == invoiceDB.Id &&
                itemInvoice.Id >= currentSalsItemIndex);

                if (itemsInvoice != null &&
                itemsInvoice.Any())
                {
                    itemsInvoice = itemsInvoice.OrderByDescending(i => i.Id);

                    itemsInvoice.ToList().ForEach(i =>
                    {
                        var itemInvoice = new ItemInvoiceDB()
                        {
                            Id = i.Id + 1,
                            InvoiceId = i.InvoiceId,
                            IsSirovina = i.IsSirovina,
                            ItemCode = i.ItemCode,
                            Label = i.Label,
                            Name = i.Name,
                            OriginalUnitPrice = i.OriginalUnitPrice,
                            Quantity = i.Quantity,
                            TotalAmout = i.TotalAmout,
                            UnitPrice = i.UnitPrice,
                        };

                        sqliteDbContext.ItemInvoices.Remove(i);
                        sqliteDbContext.ItemInvoices.Add(itemInvoice);
                        sqliteDbContext.SaveChanges();
                    });
                }

                itemInvoiceDB = new ItemInvoiceDB()
                {
                    Id = currentSalsItemIndex,
                    InvoiceId = invoiceDB.Id,
                    IsSirovina = 1,
                    ItemCode = itemDB.Id,
                    Label = itemDB.Label,
                    Name = itemDB.Name,
                    Quantity = quantity,
                    OriginalUnitPrice = itemDB.SellingUnitPrice,
                    UnitPrice = itemDB.SellingUnitPrice,
                    TotalAmout = itemDB.SellingUnitPrice * quantity
                };

                sqliteDbContext.ItemInvoices.Add(itemInvoiceDB);
            }
            sqliteDbContext.SaveChangesAsync();
        }
        private void ReduceNormHasNorm(SqliteDbContext sqliteDbContext,
            InvoiceDB invoice,
            ItemDB itemDB,
            ItemInvoiceDB itemInInvoice,
            List<ItemInNormForChange> itemInNormForChanges)
        {

            if (itemInInvoice.UnitPrice != null &&
                itemInInvoice.UnitPrice.HasValue &&
                itemInInvoice.Quantity != null &&
                itemInInvoice.Quantity.HasValue)
            {
                if (invoice.TransactionType == 0)
                {
                    itemDB.TotalQuantity += itemInInvoice.Quantity.Value;
                }
                else
                {
                    itemDB.TotalQuantity -= itemInInvoice.Quantity.Value;
                }
                sqliteDbContext.Items.Update(itemDB);

                if (itemInNormForChanges.FirstOrDefault(i => i.ItemId == itemInInvoice.ItemCode &&
                i.InvoiceId == invoice.Id) == null)
                {
                    itemInNormForChanges.Add(new ItemInNormForChange()
                    {
                        ItemId = itemDB.Id,
                        UnitPrice = itemInInvoice.UnitPrice.Value,
                        InvoiceId = invoice.Id
                    });
                }
                sqliteDbContext.Remove(itemInInvoice);
                sqliteDbContext.SaveChanges();
            }
        }
        private void ReduceNormNoNorm(SqliteDbContext sqliteDbContext,
            InvoiceDB invoice,
            ItemDB itemDB,
            decimal quantity,
            List<ItemInNormForChange> itemInNormForChanges)
        {
            if (invoice.TransactionType == 0)
            {
                itemDB.TotalQuantity += quantity;
            }
            else
            {
                itemDB.TotalQuantity -= quantity;
            }
            sqliteDbContext.Items.Update(itemDB);

            if (itemInNormForChanges.FirstOrDefault(i => i.ItemId == itemDB.Id &&
            i.InvoiceId == invoice.Id) == null)
            {
                itemInNormForChanges.Add(new ItemInNormForChange()
                {
                    ItemId = itemDB.Id,
                    UnitPrice = itemDB.SellingUnitPrice,
                    InvoiceId = invoice.Id
                });
            }
            sqliteDbContext.SaveChanges();
        }
        private void IncreaseNorm(SqliteDbContext sqliteDbContext,
        InvoiceDB invoice,
        ItemDB itemDB,
        decimal quantity,
        List<ItemInNormForChange> itemInNormForChanges)
        {
            if (invoice.TransactionType == 0)
            {
                itemDB.TotalQuantity -= quantity;
            }
            else
            {
                itemDB.TotalQuantity += quantity;
            }
            sqliteDbContext.Items.Update(itemDB);

            var itemForChange = itemInNormForChanges.FirstOrDefault(i => i.ItemId == itemDB.Id &&
            i.InvoiceId == invoice.Id);

            int index = sqliteDbContext.ItemInvoices.Where(itemInvoice => itemInvoice.InvoiceId == invoice.Id).Max(i => i.Id) + 1;

            ItemInvoiceDB? itemInvoiceDB = null;
            if (itemForChange != null)
            {
                itemInvoiceDB = new ItemInvoiceDB()
                {
                    Id = index,
                    InvoiceId = invoice.Id,
                    IsSirovina = 1,
                    ItemCode = itemDB.Id,
                    Label = itemDB.Label,
                    Name = itemDB.Name,
                    Quantity = quantity,
                    OriginalUnitPrice = itemForChange.UnitPrice,
                    UnitPrice = itemForChange.UnitPrice,
                    TotalAmout = itemForChange.UnitPrice * quantity
                };
            }
            else
            {
                itemInvoiceDB = new ItemInvoiceDB()
                {
                    Id = index,
                    InvoiceId = invoice.Id,
                    IsSirovina = 1,
                    ItemCode = itemDB.Id,
                    Label = itemDB.Label,
                    Name = itemDB.Name,
                    Quantity = quantity,
                    OriginalUnitPrice = itemDB.SellingUnitPrice,
                    UnitPrice = itemDB.SellingUnitPrice,
                    TotalAmout = itemDB.SellingUnitPrice * quantity
                };
            }
            sqliteDbContext.ItemInvoices.Add(itemInvoiceDB);
            sqliteDbContext.SaveChanges();
        }

    }
    internal class ItemInNormForChange
    {
        public string? InvoiceId { get; set; }
        public string? ItemId { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}