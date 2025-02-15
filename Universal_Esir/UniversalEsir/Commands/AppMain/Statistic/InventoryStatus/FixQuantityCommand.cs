using UniversalEsir.ViewModels.AppMain.Statistic;
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

namespace UniversalEsir.Commands.AppMain.Statistic.InventoryStatus
{
    public class FixQuantityCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public FixQuantityCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();
                await sqliteDbContext.Items.ForEachAsync(itemDB =>
                {
                    decimal totalCalculationQuantity = 0;
                    decimal totalPazarQuantity = 0;

                    var itemInCal = sqliteDbContext.Calculations.Join(sqliteDbContext.CalculationItems,
                        cal => cal.Id,
                        calItem => calItem.CalculationId,
                        (cal, calItem) => new { Cal = cal, CalItem = calItem })
                    .Where(cal => cal.CalItem.ItemId == itemDB.Id);

                    var pazari = sqliteDbContext.Invoices.Join(sqliteDbContext.ItemInvoices,
                        invoice => invoice.Id,
                        invoiceItem => invoiceItem.InvoiceId,
                        (invoice, invoiceItem) => new { Inv = invoice, InvItem = invoiceItem })
                        .Where(pazar => pazar.Inv.SdcDateTime != null && pazar.Inv.SdcDateTime.HasValue &&
                        pazar.InvItem.ItemCode == itemDB.Id)
                        .OrderByDescending(item => item.Inv.SdcDateTime);

                    if (itemInCal != null &&
                    itemInCal.Any())
                    {
                        totalCalculationQuantity = itemInCal.Select(i => i.CalItem.Quantity).ToList().Sum();
                    }

                    if (pazari != null &&
                    pazari.Any())
                    {
                        var list = pazari.Select(i => i.Inv.TransactionType != null && i.Inv.TransactionType == 0 &&
                        i.InvItem.Quantity != null && i.InvItem.Quantity.HasValue ?
                        i.InvItem.IsSirovina == 1 ? i.InvItem.Quantity.Value :
                        itemDB.IdNorm == null ? i.InvItem.Quantity.Value : 0 : 0).ToList();

                        var listRefundacija = pazari.Select(i => i.Inv.TransactionType != null && i.Inv.TransactionType == 1 &&
                        i.InvItem.Quantity != null && i.InvItem.Quantity.HasValue ?
                        i.InvItem.IsSirovina == 1 ? i.InvItem.Quantity.Value :
                        itemDB.IdNorm == null ? i.InvItem.Quantity.Value : 0 : 0).ToList();

                        totalPazarQuantity += list.Sum() - listRefundacija.Sum();
                    }

                    itemDB.TotalQuantity = totalCalculationQuantity - totalPazarQuantity;
                    sqliteDbContext.Items.Update(itemDB);

                    sqliteDbContext.SaveChanges();
                });

                MessageBox.Show("Uspešno sređivanje količina artikala!",
                    "Uspešno",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Greška prilikom sređivanja količina artikala!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}