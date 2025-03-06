﻿using UniversalEsir_Common.Enums;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Common.Models.Invoice.FileSystemWatcher;
using UniversalEsir_Common.Models.Order;
using UniversalEsir_Common.Models.Statistic;
using UniversalEsir_Printer.Enums;
using UniversalEsir_Report;
using UniversalEsir_Report.Models;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_Printer.Models;

namespace UniversalEsir_Printer.PaperFormat
{
    public class FormatPos
    {
        private static MorePage? _morePage;

        private static decimal _totalAmount;
        private static string _journal;
        private static string _reportString;
        private static string _inventoryStatus;
        private static string _verificationQRCode;
        private static int _width;
        private static int _height;
        private static float _fontSizeInMM;
        private static readonly int _sizeQRmm = 45;
        private static readonly float _fontSize80mm = 3.90f;
        private static readonly float _fontSize58mm = 2.90f;

        private static string CreateOrder(Order order)
        {
            string oredr = CenterString("ПОРУЏБИНА", 28);
            oredr += "============================\r\n";
            oredr += SplitInParts($"{order.CashierName}", "Конобар:", 28);
            oredr += SplitInParts($"{order.OrderTime.ToString("dd.MM.yyyy HH:mm:ss")}", "Време:", 28);
            oredr += SplitInParts($"{order.TableId}", "Сто:", 28);
            oredr += SplitInParts($"{order.PartHall}", "Део сале:", 28);
            oredr += "============================\r\n";
            oredr += CenterString("Артикли", 28);
            oredr += string.Format("{0}{1}\r\n", "Назив".PadRight(18), "Кол.".PadLeft(10));
            oredr += "----------------------------\r\n";

            foreach (var item in order.Items)
            {
                string i = string.Format("{0}", item.Name);

                oredr += SplitInParts(i, "", 28, 1);
                oredr += string.Format("{0}{1}\r\n", string.Empty.PadRight(18),
                    string.Format("{0:#,##0.00}", item.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));
            }
            oredr += "============================\r\n\r\n";

            return oredr;
        }

        private static string GetItemsForJournal(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            string items = "Артикли\r\n";
            items += "============================\r\n";
            items += string.Format("{0}{1}{2}{3}\r\n", "Назив".PadRight(7), "Цена".PadRight(7), "Кол.".PadRight(4), "Укупно".PadLeft(10));

            foreach (ItemFileSystemWatcher item in invoiceRequest.Items)
            {
                string i = string.Format("{0}", item.Name);

                decimal price = item.TotalAmount / item.Quantity;

                items += SplitInParts(i, "", 28, 1);
                items += string.Format("{0}{1}{2}{3}\r\n", string.Empty.PadRight(7),
                    string.Format("{0:#,##0.00}", price).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadRight(7),
                    item.Quantity.ToString().PadRight(4),
                    string.Format("{0:#,##0.00}", item.TotalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));

                _totalAmount += item.TotalAmount;
            }

            return items;
        }

        public static void PrintInventoryStatus(PrinterFormatEnumeration printerFormat, 
            List<InvertoryGlobal> inventoryStatusAll, 
            string title,
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {



            decimal totalInputPriceCal = 0;
            decimal totalSellingPriceCal = 0;

            _inventoryStatus = "==============================\r\n";
            _inventoryStatus += CenterString(title, 30);
            _inventoryStatus += CenterString($"Datum: {dateTime.ToString("dd.MM.yyyy")}", 30);

            if (supplierGlobal != null)
            {

                if (!string.IsNullOrEmpty(supplierGlobal.InvoiceNumber))
                {
                    _inventoryStatus += "==============================\r\n";
                    _inventoryStatus += CenterString(supplierGlobal.InvoiceNumber, 30);
                    _inventoryStatus += "==============================\r\n";
                }
                else
                {
                    _inventoryStatus += "==============================\r\n";
                }

                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Name) ? string.Empty : SplitInParts($"{supplierGlobal.Name}", "Naziv dobavljača:", 30);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Pib) ? string.Empty : SplitInParts($"{supplierGlobal.Pib}", "PIB:", 30);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Mb) ? string.Empty : SplitInParts($"{supplierGlobal.Mb}", "MB:", 30);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.City) ? string.Empty : SplitInParts($"{supplierGlobal.City}", "Grad:", 30);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Address) ? string.Empty : SplitInParts($"{supplierGlobal.Address}", "Adresa:", 30);
                _inventoryStatus += string.IsNullOrEmpty(supplierGlobal.Email) ? string.Empty : SplitInParts($"{supplierGlobal.Email}", "E-mail:", 30);

                _inventoryStatus += "==============================\r\n";
            }

            _inventoryStatus += "                                        \r\n";
            inventoryStatusAll.ForEach(inventory =>
            {
                _inventoryStatus += SplitInParts($"{inventory.Id}", "Šifra:", 30);
                _inventoryStatus += SplitInParts($"{inventory.Name}", "Naziv:", 30);
                _inventoryStatus += SplitInParts($"{inventory.Jm}", "JM:", 30);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.Quantity).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Količina:", 30);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.InputUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Jed. ulazna cena:", 30);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.SellingUnitPrice).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Jed. prodajna cena:", 30);
                _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", inventory.TotalAmout).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Prodajna vrednost:", 30);

                _inventoryStatus += "                                        \r\n";

                totalInputPriceCal += Decimal.Round(inventory.InputUnitPrice * inventory.Quantity, 2);
                totalSellingPriceCal += Decimal.Round(inventory.SellingUnitPrice * inventory.Quantity, 2);
            });

            _inventoryStatus += "==============================\r\n";

            _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", totalInputPriceCal).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Ukupan ulaz:", 30);
            _inventoryStatus += SplitInParts($"{string.Format("{0:#,##0.00}", totalSellingPriceCal).Replace(',', '#').Replace('.', ',').Replace('#', '.')}", "Ukupan izlaz:", 30);

            _inventoryStatus += "==============================\r\n";

            string? prName = SettingsManager.Instance.GetPrinterName();

            if (!string.IsNullOrEmpty(prName))
            {
                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                _height = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Height / 100 * 25.4);
                switch (printerFormat)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        _fontSizeInMM = _fontSize58mm;
                        if (width > 52)
                        {
                            width = 52;
                        }
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        _fontSizeInMM = _fontSize80mm;
                        if (width > 72)
                        {
                            width = 72;
                        }
                        break;
                }
                _width = width;

                _morePage = null;

                pdoc.PrintPage += new PrintPageEventHandler(dailyDepInventory);
                pdoc.Print();
                pdoc.PrintPage -= new PrintPageEventHandler(dailyDepInventory);

                _morePage = null;
            }
        }

        public static void PrintOrder(Order order, PosTypeEnumeration posTypeEnumeration, OrderTypeEnumeration orderTypeEnumeration)
        {
            string? prName = null;

            if(orderTypeEnumeration == OrderTypeEnumeration.Sank)
            {
                var name = SettingsManager.Instance.GetPrinterNameSank1();

                if (!string.IsNullOrEmpty(name))
                {
                    prName = SettingsManager.Instance.GetPrinterNameSank1();
                }
            }
            else
            {
                var name = SettingsManager.Instance.GetPrinterNameKuhinja();

                if (!string.IsNullOrEmpty(name))
                {
                    prName = SettingsManager.Instance.GetPrinterNameKuhinja();
                }
            }

            if (!string.IsNullOrEmpty(prName))
            {
                _journal = CreateOrder(order);

                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                pdoc.PrinterSettings.PrinterName = prName;

                int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                switch (posTypeEnumeration)
                {
                    case PosTypeEnumeration.Pos80mm:
                        _fontSizeInMM = _fontSize80mm;

                        if (width > 70)
                        {
                            width = 70;
                        }

                        _width = width;
                        break;
                    case PosTypeEnumeration.Pos58mm:
                        _fontSizeInMM = _fontSize58mm;

                        if (width > 50)
                        {
                            width = 50;
                        }

                        _width = width;
                        break;
                }

                pdoc.PrintPage += new PrintPageEventHandler(dailyDep);

                pdoc.Print();

                pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
            }
        }

        public static void PrintJournalBlack(InvoceRequestFileSystemWatcher invoiceRequest, PosTypeEnumeration posTypeEnumeration)
        {
            _totalAmount = 0;
            try
            {
                _journal = string.Format("Касир:{0}\r\n", invoiceRequest.Cashier.PadLeft(28 - "Касир:".Length));
                _journal += string.Format("Члан:{0}\r\n", invoiceRequest.ClanName.PadLeft(28 - "Члан:".Length));
                _journal += string.Format("Време:{0}\r\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss").PadLeft(28 - "Време:".Length));
                _journal += GetItemsForJournal(invoiceRequest);

                _journal += "----------------------------\r\n";

                _journal += string.Format("Укупан износ:{0}\r\n", string.Format("{0:#,##0.00}", _totalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(28 - "Укупан износ:".Length));

                if(invoiceRequest.Payment.PaymentType == PaymentTypeEnumeration.Cash)
                {
                    _journal += string.Format("{0}{1}\r\n", "Готовина:".PadRight(18),
                        string.Format("{0:#,##0.00}", _totalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));
                }
                else
                {
                    _journal += string.Format("{0}{1}\r\n", "Црта:".PadRight(18),
                        string.Format("{0:#,##0.00}", _totalAmount).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(10));
                }

                _journal += "============================\r\n";


                string? prName = SettingsManager.Instance.GetPrinterName();

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    PrinterSettings ps = new PrinterSettings();
                    pdoc.PrinterSettings.PrinterName = prName;

                    int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                    switch (posTypeEnumeration)
                    {
                        case PosTypeEnumeration.Pos80mm:
                            _fontSizeInMM = _fontSize80mm;

                            if (width > 72)
                            {
                                width = 72;
                            }

                            _width = width;
                            break;
                        case PosTypeEnumeration.Pos58mm:
                            _fontSizeInMM = _fontSize58mm;

                            if (width > 50)
                            {
                                width = 50;
                            }

                            _width = width;
                            break;
                    }

                    pdoc.PrintPage += new PrintPageEventHandler(dailyDep);

                    for(int i = 0; i < SettingsManager.Instance.GetNumberCopy(); i++)
                    {
                        pdoc.Print();
                    }

                    pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
                }
            }
            catch { }
        }

        //public static void PrintJournal(InvoiceResult invoiceResult, InvoiceRequest invoiceRequest, PosTypeEnumeration posTypeEnumeration)
        //{
        //    try
        //    {
        //        invoiceResult.CreateVerificationQRCode();
        //        _journal = JournalHelper.CreateJournal(invoiceRequest, invoiceResult);
        //        _verificationQRCode = invoiceResult.VerificationQRCode;

        //        string? prName = SettingsManager.Instance.GetPrinterName();

        //        if (!string.IsNullOrEmpty(prName))
        //        {
        //            var pdoc = new PrintDocument();
        //            PrinterSettings ps = new PrinterSettings();
        //            pdoc.PrinterSettings.PrinterName = prName;

        //            int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
        //            switch (posTypeEnumeration)
        //            {
        //                case PosTypeEnumeration.Pos80mm:
        //                    _fontSizeInMM = _fontSize80mm;

        //                    if (width > 72)
        //                    {
        //                        width = 72;
        //                    }

        //                    _width = width;
        //                    break;
        //                case PosTypeEnumeration.Pos58mm:
        //                    _fontSizeInMM = _fontSize58mm;

        //                    if (width > 50)
        //                    {
        //                        width = 50;
        //                    }

        //                    _width = width;
        //                    break;
        //            }

        //            pdoc.PrintPage += new PrintPageEventHandler(dailyDep);
        //            pdoc.Print();
        //            pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
        //        }
        //    }
        //    catch { }
        //}
        public static void PrintReport(string report, PosTypeEnumeration posTypeEnumeration)
        {
            try
            {
                _reportString = report;

                string? prName = SettingsManager.Instance.GetPrinterName();

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    PrinterSettings ps = new PrinterSettings();
                    pdoc.PrinterSettings.PrinterName = prName;

                    int width = Convert.ToInt32(pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width / 100 * 25.4);
                    switch (posTypeEnumeration)
                    {
                        case PosTypeEnumeration.Pos80mm:
                            _fontSizeInMM = _fontSize80mm;

                            if (width > 70)
                            {
                                width = 70;
                            }

                            _width = width;
                            break;
                        case PosTypeEnumeration.Pos58mm:
                            _fontSizeInMM = _fontSize58mm;

                            if (width > 50)
                            {
                                width = 50;
                            }

                            _width = width;
                            break;
                    }

                    pdoc.PrintPage += new PrintPageEventHandler(dailyDepReport);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(dailyDepReport);
                }
            }
            catch { }
        }
        public static string CreateReportBlack(Report report)
        {
            string reportText = "============================\r\n";

            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Presek stanja", 28);
                    }
                    else
                    {
                        reportText += CenterString("Dnevni izveštaj", 28);
                    }
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 28);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Dnevni izveštaj", 28);
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 28);
                }
            }


            reportText += SplitInParts($"{report.StartReport.ToString("dd.MM.yyyy. HH:mm")}", "Početak:", 28);
            reportText += SplitInParts($"{report.EndReport.ToString("dd.MM.yyyy. HH:mm")}", "Kraj:", 28);

            reportText += "============================\r\n";
            reportText += "                            \r\n";
            reportText += "                            \r\n";

            //reportText += "=================TAKSE==================\r\n";
            //reportText += ReportReportTaxes(report.ReportTaxes);
            //reportText += "========================================\r\n";

            //reportText += "============NAČINI PLAĆANJA=============\r\n";
            //reportText += ReportPayments(report.Payments);
            //reportText += "========================================\r\n";

            if (report.ReportItems.Any())
            {
                reportText += "==========ARTIKLI===========\r\n";
                reportText += ReportReportItems(report.ReportItems);
                reportText += "============================\r\n";
            }

            reportText += "===========KASIRI===========\r\n";
            reportText += ReportCashiers(report.ReportCashiers);
            reportText += "============================\r\n";

            //if (report.InvoiceTypes.Any())
            //{
            //    reportText += "=========PROMET PO VRSTI RAČUNA=========\r\n";
            //    reportText += ReportInvoiceTypes(report.InvoiceTypes);
            //    reportText += "========================================\r\n";
            //}

            reportText += "=======Gotovina u kasi======\r\n";
            reportText += SplitInParts("Bruto", "Valuta", 28);
            reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.CashInHand).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "RSD", 28);
            //reportText += "========================================\r\n";

            //reportText += SplitInParts($"{report.TotalTraffic.ToString("00.00")} din", "Promet:", 40);
            reportText += "                            \r\n";
            reportText += "============================\r\n";
            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Kraj presek stanja", 28);
                    }
                    else
                    {
                        reportText += CenterString("Kraj dnevnog izveštaja", 28);
                    }
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 28);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Kraj dnevnog izveštaja", 28);
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 28);
                }
            }
            reportText += "============================\r\n";

            return reportText;
        }
        public static string CreateReport(Report report)
        {
            string reportText = "============================\r\n";
            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Presek stanja", 28);
                    }
                    else
                    {
                        reportText += CenterString("Dnevni izveštaj", 28);
                    }
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 28);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Dnevni izveštaj", 28);
                }
                else
                {
                    reportText += CenterString("Periodični izveštaj", 28);
                }
            }


            reportText += SplitInParts($"{report.StartReport.ToString("dd.MM.yyyy. HH:mm")}", "Početak:", 28);
            reportText += SplitInParts($"{report.EndReport.ToString("dd.MM.yyyy. HH:mm")}", "Kraj:", 28);
            reportText += "============================\r\n";
            reportText += "                            \r\n";
            reportText += "                            \r\n";

            //reportText += "=================TAKSE==================\r\n";
            //reportText += ReportReportTaxes(report.ReportTaxes);
            //reportText += "========================================\r\n";


            reportText += "===========KASIRI===========\r\n";
            reportText += ReportCashiers(report.ReportCashiers);
            reportText += "============================\r\n";

            reportText += "======NAČINI PLAĆANJA=======\r\n";
            reportText += ReportPayments(report.Payments);
            reportText += "============================\r\n";

            if (report.ReportItems.Any())
            {
                reportText += "==========ARTIKLI===========\r\n";
                reportText += ReportReportItems(report.ReportItems);
                reportText += "============================\r\n";
            }

            //if (report.InvoiceTypes.Any())
            //{
            //    reportText += "=========PROMET PO VRSTI RAČUNA=========\r\n";
            //    reportText += ReportInvoiceTypes(report.InvoiceTypes);
            //    reportText += "========================================\r\n";
            //}

            reportText += "=======Gotovina u kasi======\r\n";
            reportText += SplitInParts("Bruto", "Valuta", 28);
            reportText += SplitInParts($"{string.Format("{0:#,##0.00}", report.CashInHand).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "RSD", 28);
            //reportText += "========================================\r\n";

            //reportText += SplitInParts($"{report.TotalTraffic.ToString("00.00")} din", "Promet:", 40);
            reportText += "                            \r\n";
            reportText += "============================\r\n";
            if (report.StartReport.Day == report.EndReport.Day)
            {
                if (report.StartReport.Month == report.EndReport.Month)
                {
                    DateTime dateTime = new DateTime(report.EndReport.Year, report.EndReport.Month, report.EndReport.Day, 23, 59, 59);

                    if (report.EndReport < dateTime)
                    {
                        reportText += CenterString("Kraj presek stanja", 28);
                    }
                    else
                    {
                        reportText += CenterString("Kraj dnevnog izveštaja", 28);
                    }
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 28);
                }
            }
            else
            {
                if (report.EndReport.Subtract(report.StartReport) < new TimeSpan(29, 0, 0))
                {
                    reportText += CenterString("Kraj dnevnog izveštaja", 28);
                }
                else
                {
                    reportText += CenterString("Kraj periodičnog izveštaja", 28);
                }
            }
            reportText += "============================\r\n";

            return reportText;
        }

        private static void dailyDepInventory(object sender, PrintPageEventArgs e)
        {
            const float neededHeight = 773.7007874F;
            Graphics graphics = e.Graphics;
            graphics.PageUnit = GraphicsUnit.Point;
            Font drawFontRegular = new Font("Cascadia Code",
                _fontSizeInMM,
                System.Drawing.FontStyle.Bold, GraphicsUnit.Millimeter);
            SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

            string[] splitForPrint = _inventoryStatus.Split("\r\n");

            float x = 0;
            float y = 0;
            float width = 0; // max width I found through trial and error
            float height = 0F;
            int startIndex = 0;
            int length = splitForPrint.Length;

            if (_morePage is null)
            {
                if (splitForPrint[length - 1].Length == 0)
                {
                    length--;
                }
            }
            else
            {
                startIndex = _morePage.Index;
            }

            for (int i = startIndex; i < length; i++)
            {
                var input = y + graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
                if (input < neededHeight)
                {
                    graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                    y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
                }
                else
                {
                    e.HasMorePages = true;
                    _morePage = new MorePage()
                    {
                        Type = Models.Type.End,
                        Index = i - 1
                    };
                    return;
                }

            }
        }

        private static void dailyDepReport(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.PageUnit = GraphicsUnit.Point;
            Font drawFontRegular = new Font("Cascadia Code",
                _fontSizeInMM,
                System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
            SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

            string[] splitForPrint = _reportString.Split("\r\n");

            float x = 0;
            float y = 0;
            float width = 0; // max width I found through trial and error
            float height = 0F;
            int length = splitForPrint.Length;

            if (splitForPrint[length - 1].Length == 0)
            {
                length--;
            }

            bool first = true;

            for (int i = 0; i < length; i++)
            {
                if (first)
                {
                    first = false;

                    width = graphics.MeasureString(splitForPrint[i], drawFontRegular).Width;
                    height = graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;

                    float v = _width / 100f;
                    x = (v * 72 - width) / 2;
                    if (x < 5)
                    {
                        x = 5;
                    }

                    string strBLOBFilePath = SettingsManager.Instance.GetPathToLogo();

                    if (File.Exists(strBLOBFilePath))
                    {
                        FileStream fsBLOBFile = new FileStream(strBLOBFilePath, FileMode.Open, FileAccess.Read);
                        Byte[] bytBLOBData = new Byte[fsBLOBFile.Length];
                        fsBLOBFile.Read(bytBLOBData, 0, bytBLOBData.Length);
                        fsBLOBFile.Close();
                        using (MemoryStream ms = new MemoryStream(bytBLOBData))
                        {
                            var img = System.Drawing.Image.FromStream(ms);

                            var size = width * 0.30F;
                            var xx = x + width * 0.35F;
                            graphics.DrawImage(img, new RectangleF(xx, y + height, size, size));
                            y += 2 * height + size;
                        }
                    }
                }

                graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
            }
        }

        private static void dailyDep(object sender, PrintPageEventArgs e)
        {
            try
            {
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontUpperBold = new Font("Cascadia Code",
                    drawFontRegular.SizeInPoints * 1.5f,
                    System.Drawing.FontStyle.Bold, GraphicsUnit.Point);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);

                string[] splitForPrint = _journal.Split("\r\n");

                float x = 0;
                float y = 0;
                float width = 0; // max width I found through trial and error
                float height = 0F;
                int length = splitForPrint.Length;

                if (splitForPrint[length - 1].Length == 0)
                {
                    length--;
                }

                for (int i = 0; i < length - 1; i++)
                {
                    if (width == 0)
                    {
                        width = graphics.MeasureString(splitForPrint[i], drawFontRegular).Width;
                        height = graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;

                        float v = _width / 100f;
                        x = (v * 72 - width) / 2;
                        if (x < 5)
                        {
                            x = 5;
                        }

                        string strBLOBFilePath = SettingsManager.Instance.GetPathToLogo();

                        if (File.Exists(strBLOBFilePath))
                        {
                            FileStream fsBLOBFile = new FileStream(strBLOBFilePath, FileMode.Open, FileAccess.Read);
                            Byte[] bytBLOBData = new Byte[fsBLOBFile.Length];
                            fsBLOBFile.Read(bytBLOBData, 0, bytBLOBData.Length);
                            fsBLOBFile.Close();
                            using (MemoryStream ms = new MemoryStream(bytBLOBData))
                            {
                                var img = System.Drawing.Image.FromStream(ms);

                                var size = width * 0.30F;
                                var xx = x + width * 0.35F;
                                graphics.DrawImage(img, new RectangleF(xx, y + height, size, size));
                                y += 2 * height + size;
                            }
                        }
                    }
                    if (splitForPrint[i].Contains("ОВО НИЈЕ ФИСКАЛНИ РАЧУН") &&
                        !splitForPrint[i].Contains("="))
                    {
                        float xLeft = (width - graphics.MeasureString(splitForPrint[i], drawFontUpperBold).Width) / 2f;

                        graphics.DrawString(splitForPrint[i], drawFontUpperBold, drawBrush, xLeft, y);
                        y += graphics.MeasureString(splitForPrint[i], drawFontUpperBold).Height;
                    }
                    else
                    {
                        graphics.DrawString(splitForPrint[i], drawFontRegular, drawBrush, x, y);
                        y += graphics.MeasureString(splitForPrint[i], drawFontRegular).Height;
                    }
                }

                byte[] byteBuffer = Convert.FromBase64String(_verificationQRCode);
                using (MemoryStream ms = new MemoryStream(byteBuffer))
                {
                    var img = System.Drawing.Image.FromStream(ms);

                    var size = _sizeQRmm * 2.8346456693F;
                    var xx = x + (width - size) / 2F;
                    graphics.DrawImage(img, new RectangleF(xx, y + height, size, size));
                    y += 2 * height + size;
                }
                graphics.DrawString(splitForPrint[length - 1], drawFontRegular, drawBrush, x, y);
            }
            catch (Exception ex)
            {

            }
        }
        //private static string ReportReportTaxes(Dictionary<string, ReportTax> reportTaxes)
        //{
        //    string result = string.Empty;

        //    decimal totalGross = 0;
        //    decimal totalPdv = 0;
        //    decimal totalNet = 0;

        //    foreach (KeyValuePair<string, ReportTax> item in reportTaxes)
        //    {
        //        result += SplitInParts($"{item.Key} ({item.Value.Rate}%)", "PDV grupa:", 40);
        //        result += SplitInParts($"{item.Value.Gross.ToString("00.00")} din", "Bruto:", 40);
        //        result += SplitInParts($"{item.Value.Pdv.ToString("00.00")} din", "PDV:", 40);
        //        result += SplitInParts($"{item.Value.Net.ToString("00.00")} din", "Neto:", 40);

        //        result += "                                        \r\n";

        //        totalGross += item.Value.Gross;
        //        totalPdv += item.Value.Pdv;
        //        totalNet += item.Value.Net;
        //    }

        //    result += "---------------- Ukupno ----------------\r\n";
        //    result += SplitInParts($"{totalGross.ToString("00.00")} din", "Bruto:", 40);
        //    result += SplitInParts($"{totalPdv.ToString("00.00")} din", "PDV:", 40);
        //    result += SplitInParts($"{totalNet.ToString("00.00")} din", "Neto:", 40);

        //    return result;
        //}
        private static string ReportPayments(List<Payment> payments)
        {
            string result = string.Empty;

            decimal total = 0;

            payments.ForEach(x =>
            {
                string paymentType = string.Empty;

                switch (x.PaymentType)
                {
                    case PaymentTypeEnumeration.Cash:
                        paymentType = "Gotovina";
                        break;
                    case PaymentTypeEnumeration.Crta:
                        paymentType = "Crta";
                        break;
                    case PaymentTypeEnumeration.Check:
                        paymentType = "Ček";
                        break;
                    case PaymentTypeEnumeration.Voucher:
                        paymentType = "Vaučer";
                        break;
                    case PaymentTypeEnumeration.Other:
                        paymentType = "Drugo bezgotovinsko plaćanje";
                        break;
                    case PaymentTypeEnumeration.WireTransfer:
                        paymentType = "Prenos na račun";
                        break;
                    case PaymentTypeEnumeration.MobileMoney:
                        paymentType = "Instant plaćanje";
                        break;
                }

                result += SplitInParts($"{paymentType}", "Način plaćanja:", 28);
                result += SplitInParts($"{string.Format("{0:#,##0.00}", x.Amount).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 28);

                result += "                       \r\n";

                total += x.Amount;
            });

            result += "---------- Ukupno ----------\r\n";
            result += SplitInParts($"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 28);

            return result;
        }
        private static string ReportReportItems(Dictionary<string, Dictionary<string, ReportItem>> reportItems)
        {
            string result = string.Empty;

            decimal total = 0;

            foreach (KeyValuePair<string, Dictionary<string, ReportItem>> group in reportItems)
            {
                result += CenterString(group.Key, 28);

                foreach (KeyValuePair<string, ReportItem> item in group.Value)
                {
                    result += SplitInParts($"{item.Key}", "Šifra:", 28);
                    result += SplitInParts($"{item.Value.Name}", "Artikal:", 28);
                    result += SplitInParts($"{item.Value.Quantity}", "Količina:", 28);
                    result += SplitInParts($"{item.Value.Gross} din", "Bruto:", 28);

                    result += "                            \r\n";

                    total += item.Value.Gross;
                }
                result += "----------------------------\r\n";
            }

            result += "---------- Ukupno ----------\r\n";
            result += SplitInParts($"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 28);

            return result;
        }
        private static string ReportCashiers(Dictionary<string, decimal> cashiers)
        {
            string result = string.Empty;

            decimal total = 0;

            foreach (KeyValuePair<string, decimal> item in cashiers)
            {
                result += SplitInParts($"{item.Key}", "Kasir:", 28);
                result += SplitInParts($"{string.Format("{0:#,##0.00}", item.Value).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 28);

                result += "                            \r\n";

                total += item.Value;
            }

            result += "---------- Ukupno ----------\r\n";
            result += SplitInParts($"{string.Format("{0:#,##0.00}", total).Replace(',', '#').Replace('.', ',').Replace('#', '.')} din", "Bruto:", 28);

            return result;
        }
        //private static string ReportInvoiceTypes(Dictionary<InvoiceTypeEenumeration, List<ReportInvoiceType>> invoiceTypes)
        //{
        //    string result = string.Empty;

        //    decimal total = 0;

        //    foreach (KeyValuePair<InvoiceTypeEenumeration, List<ReportInvoiceType>> item in invoiceTypes)
        //    {
        //        string invoiceType = string.Empty;

        //        switch (item.Key)
        //        {
        //            case InvoiceTypeEenumeration.Normal:
        //                invoiceType = "Promet";
        //                break;
        //            case InvoiceTypeEenumeration.Proforma:
        //                invoiceType = "Predračun";
        //                break;
        //            case InvoiceTypeEenumeration.Copy:
        //                invoiceType = "Kopija";
        //                break;
        //            case InvoiceTypeEenumeration.Training:
        //                invoiceType = "Obuka";
        //                break;
        //            case InvoiceTypeEenumeration.Advance:
        //                invoiceType = "Avans";
        //                break;
        //        }
        //        result += SplitInParts("Bruto", "Kasir", 40);

        //        result += CenterString(invoiceType, 40);
        //        item.Value.ForEach(type =>
        //        {
        //            result += SplitInParts($"{type.Gross.ToString("00.00")} din", $"{type.Cashier}", 40);

        //            total += type.Gross;
        //        });

        //        result += "                                        \r\n";
        //    }

        //    result += "---------------- Ukupno ----------------\r\n";
        //    result += SplitInParts($"{total.ToString("00.00")} din", "Bruto:", 40);

        //    return result;
        //}

        private static string CenterString(string value, int length)
        {
            string journal = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (value.Length < length)
            {
                int spaces = length - value.Length;
                int padLeft = spaces / 2 + value.Length;

                return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
            }

            string str = value;
            int journalLength = value.Length;

            int counter = 0;

            while (journalLength > 0)
            {
                int len = 0;
                if (journalLength > length)
                {
                    len = length;
                }
                else
                {
                    len = journalLength;
                }
                string s = str.Substring(counter * length, len);

                int spaces = length - s.Length;
                int padLeft = spaces / 2 + s.Length;

                journal += $"{s.PadLeft(padLeft).PadRight(length)}\r\n";

                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
        private static string SplitInParts(string value, string fixedPart, int length, int pad = 0)
        {
            string journal = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }

            if (fixedPart.Length + value.Length <= length)
            {
                if (pad == 0)
                {
                    journal = string.Format("{0}{1}\r\n", fixedPart, value.PadLeft(length - fixedPart.Length));
                }
                else
                {
                    journal = string.Format("{0}{1}\r\n", fixedPart, value.PadRight(length));
                }
                return journal;
            }

            string str = fixedPart + value;

            int journalLength = str.Length;

            int counter = 0;

            while (journalLength > 0)
            {
                int len = 0;
                if (journalLength > length)
                {
                    len = length;
                }
                else
                {
                    len = journalLength;
                }
                string s = str.Substring(counter * length, len);

                journal += string.Format("{0}\r\n", s.PadRight(length));

                journalLength -= s.Length;
                counter++;
            }

            return journal;
        }
    }
}
