using UniversalEsir.Enums.Sale;
using UniversalEsir.Models.AppMain.Statistic.Knjizenje;
using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Common.Enums;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Knjizenje
{
    public class SearchInvoicesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SearchInvoicesCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel is KnjizenjeViewModel)
            {
                KnjizenjePazara();
            }
            else if (_currentViewModel is PregledPazaraViewModel)
            {
                PregledPazara();
            }
            else
            {
                return;
            }
        }
        private void KnjizenjePazara()
        {
            KnjizenjeViewModel knjizenjeViewModel = (KnjizenjeViewModel)_currentViewModel;

            if (knjizenjeViewModel.CurrentDate.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Datum mora biti u sadašnjisti ili prošlosti!", "Datum je u budućnosti", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            knjizenjeViewModel.Invoices = new ObservableCollection<Invoice>();

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            knjizenjeViewModel.CurrentKnjizenjePazara = new KnjizenjePazara(knjizenjeViewModel.CurrentDate);

            var invoices = sqliteDbContext.Invoices.Where(invoice => invoice.SdcDateTime != null && invoice.SdcDateTime.HasValue &&
            invoice.SdcDateTime.Value.Date == knjizenjeViewModel.CurrentDate.Date &&
            string.IsNullOrEmpty(invoice.KnjizenjePazaraId));

            if (invoices != null &&
                invoices.Any())
            {
                invoices.ForEachAsync(invoice =>
                {
                    var cashier = sqliteDbContext.Cashiers.Find(invoice.Cashier);

                    if (cashier != null)
                    {
                        var inv = new Invoice(invoice, knjizenjeViewModel.Invoices.Count + 1);

                        inv.Cashier = cashier.Name;
                        if (invoice.InvoiceType != null && invoice.InvoiceType.HasValue &&
                        invoice.InvoiceType.Value == (int)InvoiceTypeEenumeration.Normal)
                        {
                            UpdatePaymentType(sqliteDbContext, knjizenjeViewModel.CurrentKnjizenjePazara, invoice);
                            knjizenjeViewModel.Invoices.Add(inv);
                        }
                    }
                });
                knjizenjeViewModel.Invoices = new ObservableCollection<Invoice>(knjizenjeViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
        }
        private void PregledPazara()
        {
            PregledPazaraViewModel pregledPazaraViewModel = (PregledPazaraViewModel)_currentViewModel;

            DateTime fromDate = new DateTime(pregledPazaraViewModel.FromDate.Year, pregledPazaraViewModel.FromDate.Month, pregledPazaraViewModel.FromDate.Day, 5, 0, 0);
            DateTime date = pregledPazaraViewModel.ToDate.AddDays(1);
            DateTime toDate = new DateTime(date.Year, date.Month, date.Day, 4, 59, 59);

            if(fromDate > toDate)
            {
                MessageBox.Show("Početni datum ne sme biti mlađi od krajnjeg!", "Greška u datumu", MessageBoxButton.OK, MessageBoxImage.Error);

                pregledPazaraViewModel.FromDate = DateTime.Now;
                pregledPazaraViewModel.ToDate = DateTime.Now;

                return;
            }

            if (fromDate.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Početni datum mora biti u sadašnjisti ili prošlosti!", "Datum je u budućnosti", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            pregledPazaraViewModel.Invoices = new ObservableCollection<Invoice>();

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            pregledPazaraViewModel.CurrentKnjizenjePazara = new KnjizenjePazara(fromDate);

            var invoices = sqliteDbContext.Invoices.Where(invoice => invoice.SdcDateTime != null && invoice.SdcDateTime.HasValue &&
            invoice.SdcDateTime.Value >= fromDate && invoice.SdcDateTime.Value <= toDate &&
            !string.IsNullOrEmpty(invoice.KnjizenjePazaraId));

            if (invoices != null &&
                invoices.Any())
            {
                invoices.ForEachAsync(invoice =>
                {
                    var cashier = sqliteDbContext.Cashiers.Find(invoice.Cashier);

                    if (cashier != null)
                    {
                        var inv = new Invoice(invoice, pregledPazaraViewModel.Invoices.Count + 1);

                        inv.Cashier = cashier.Name;
                        if (invoice.InvoiceType != null && invoice.InvoiceType.HasValue &&
                        invoice.InvoiceType.Value == (int)InvoiceTypeEenumeration.Normal)
                        {
                            UpdatePaymentType(sqliteDbContext, pregledPazaraViewModel.CurrentKnjizenjePazara, invoice);
                            pregledPazaraViewModel.Invoices.Add(inv);
                        }
                    }
                });

                pregledPazaraViewModel.Invoices = new ObservableCollection<Invoice>(pregledPazaraViewModel.Invoices.OrderBy(i => i.SdcDateTime));
            }
        }
        private void UpdatePaymentType(SqliteDbContext sqliteDbContext, 
            KnjizenjePazara knjizenjePazara, 
            InvoiceDB invoiceDB)
        {
            var payments = sqliteDbContext.PaymentInvoices.Where(pay => pay.InvoiceId == invoiceDB.Id);

            if(payments != null && payments.Any())
            {
                payments.ToList().ForEach(payment =>
                {
                    if (payment.Amout.HasValue)
                    {
                        if (invoiceDB.TransactionType != null &&
                        invoiceDB.TransactionType.HasValue)
                        {
                            if (invoiceDB.TransactionType.Value == (int)UniversalEsir_Common.Enums.TransactionTypeEnumeration.Refund)
                            {
                                switch (payment.PaymentType)
                                {
                                    case PaymentTypeEnumeration.Cash:
                                        knjizenjePazara.NormalRefundCash -= payment.Amout.Value;
                                        break;
                                    case PaymentTypeEnumeration.Crta:
                                        knjizenjePazara.NormalRefundCard -= payment.Amout.Value;
                                        break;
                                    case PaymentTypeEnumeration.WireTransfer:
                                        knjizenjePazara.NormalRefundWireTransfer -= payment.Amout.Value;
                                        break;
                                }
                                knjizenjePazara.Total -= payment.Amout.Value;
                            }
                            else 
                            {
                                switch (payment.PaymentType)
                                {
                                    case PaymentTypeEnumeration.Cash:
                                        knjizenjePazara.NormalSaleCash += payment.Amout.Value;
                                        break;
                                    case PaymentTypeEnumeration.Crta:
                                        knjizenjePazara.NormalSaleCard += payment.Amout.Value;
                                        break;
                                    case PaymentTypeEnumeration.WireTransfer:
                                        knjizenjePazara.NormalSaleWireTransfer += payment.Amout.Value;
                                        break;
                                }
                                knjizenjePazara.Total += payment.Amout.Value;
                            }
                        }
                    }
                });
            }
        }
    }
}