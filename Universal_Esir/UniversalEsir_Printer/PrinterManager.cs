using UniversalEsir_Common.Enums;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Common.Models.Invoice.FileSystemWatcher;
using UniversalEsir_Common.Models.Statistic;
using UniversalEsir_Common.Models.Statistic.Driver;
using UniversalEsir_Common.Models.Statistic.Nivelacija;
using UniversalEsir_Common.Models.Statistic.Norm;
using UniversalEsir_Database.Models;
using UniversalEsir_Printer.Enums;
using UniversalEsir_Printer.PaperFormat;
using UniversalEsir_Report;
using UniversalEsir_Report.Models;
using UniversalEsir_Settings;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using UniversalEsir_Printer.Models;

namespace UniversalEsir_Printer
{
    public sealed class PrinterManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static PrinterManager instance = null;
        #endregion Fields Singleton

        #region Fields
        #endregion Fields

        #region Constructors
        private PrinterManager() { }
        public static PrinterManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new PrinterManager();
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        public void PrintJournal(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        PrintPos58mm(invoiceRequest);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        PrintPos80mm(invoiceRequest);
                        break;
                }
            }
        }
        public void PrintUplata(UplataPrint uplataPrint)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        PrintUplataPos58mm(uplataPrint);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        PrintUplataPos80mm(uplataPrint);
                        break;
                }
            }
        }
        public void PrintVirman(Otpremnica otpremnica)
        {
            VirmanInvoice.PrintJournal(otpremnica);
        }
        public void PrintPonuda(Otpremnica otpremnica)
        {
            VirmanInvoice.PrintPonuda(otpremnica);
        }
        public void PrintA4InventoryStatus(List<InvertoryGlobal> inventoryStatusAll,
            string title,
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {
            FormatA4.PrintA4InventoryStatus(inventoryStatusAll, title, dateTime, supplierGlobal);
        }
        public void PrintInventoryStatus(List<InvertoryGlobal> inventoryStatusAll,
            string title,
            DateTime dateTime,
            SupplierGlobal? supplierGlobal = null)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        FormatPos.PrintInventoryStatus(PrinterFormatEnumeration.Pos58mm, inventoryStatusAll, title, dateTime, supplierGlobal);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        FormatPos.PrintInventoryStatus(PrinterFormatEnumeration.Pos80mm, inventoryStatusAll, title, dateTime, supplierGlobal);
                        break;
                }
            }
        }
        public void PrintCalculationA4Status(CalculationDB calculationDB,
            List<InvertoryGlobal> items,
            SupplierDB supplierDB)
        {
            CalculationDocument.PrintCalculation(calculationDB, items, supplierDB);
        }

        public void PrintReport(string report)
        {
            PrinterFormatEnumeration? printerFormatEnumeration = SettingsManager.Instance.GetPrinterFormat();

            if (printerFormatEnumeration != null)
            {
                switch (printerFormatEnumeration.Value)
                {
                    case PrinterFormatEnumeration.Pos58mm:
                        PrintReportPos58mm(report);
                        break;
                    case PrinterFormatEnumeration.Pos80mm:
                        PrintReportPos80mm(report);
                        break;
                }
            }
        }
        public void PrintNorms(Dictionary<string, Dictionary<string, List<NormGlobal>>> norms)
        {
            FormatA4.PrintNorms(norms);
        }
        public void PrintNivelacija(NivelacijaGlobal nivelacija)
        {
            FormatA4.PrintNivelacija(nivelacija);
        }
        public void PrintDnevniPazar(DateTime fromDateTime, DateTime? toDateTime,
            Dictionary<string, List<ReportPerItems>> allItems20PDV,
            Dictionary<string, List<ReportPerItems>> allItems10PDV,
            Dictionary<string, List<ReportPerItems>> allItems0PDV,
            Dictionary<string, List<ReportPerItems>> allItemsNoPDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovina20PDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovina10PDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovina0PDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovinaNoPDV)
        {
            FormatA4.PrintDnevniPazar(fromDateTime, toDateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV);
        }
        public void PrintIzlaz1010(DateTime fromDateTime, DateTime? toDateTime,
            Dictionary<string, List<ReportPerItems>> allItems20PDV,
            Dictionary<string, List<ReportPerItems>> allItems10PDV,
            Dictionary<string, List<ReportPerItems>> allItems0PDV,
            Dictionary<string, List<ReportPerItems>> allItemsNoPDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovina20PDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovina10PDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovina0PDV,
            Dictionary<string, List<ReportPerItems>> allItemsSirovinaNoPDV)
        {
            FormatA4.PrintIzlaz1010(fromDateTime, toDateTime,
                allItems20PDV,
                allItems10PDV,
                allItems0PDV,
                allItemsNoPDV,
                allItemsSirovina20PDV,
                allItemsSirovina10PDV,
                allItemsSirovina0PDV,
                allItemsSirovinaNoPDV);
        }
        public void PrintKEP(DateTime fromDate, DateTime toDate, List<ItemKEP> kep)
        {
            FormatA4.PrintKEP(fromDate, toDate, kep);
        }
        public void Print1010(DateTime fromDate, DateTime toDate, List<ItemKEP> kep)
        {
            FormatA4.Print1010(fromDate, toDate, kep);
        }
        public void PrintIsporuku(IsporukaGlobal isporuka)
        {
            FormatA4.PrintIsporuku(isporuka);
        }
        public void PrintAllIsporuke(List<IsporukaGlobal> isporuke, 
            DriverGlobal driver,
            string startDate,
            string endDate,
            string totalAmount)
        {
            FormatA4.PrintAllIsporuke(isporuke, driver, startDate, endDate, totalAmount);
        }
        #endregion Public methods

        #region Private methods
        private void PrintPos80mm(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            FormatPos.PrintJournalBlack(invoiceRequest, PosTypeEnumeration.Pos80mm);
        }
        private void PrintPos58mm(InvoceRequestFileSystemWatcher invoiceRequest)
        {
            FormatPos.PrintJournalBlack(invoiceRequest, PosTypeEnumeration.Pos58mm);
        }
        private void PrintUplataPos80mm(UplataPrint uplataPrint)
        {
            FormatPos.PrintUplataBlack(uplataPrint, PosTypeEnumeration.Pos80mm);
        }
        private void PrintUplataPos58mm(UplataPrint uplataPrint)
        {
            FormatPos.PrintUplataBlack(uplataPrint, PosTypeEnumeration.Pos58mm);
        }
        
        //private void PrintPos58mm(InvoiceResult invoiceResult, InvoiceRequest invoiceRequest)
        //{
        //    FormatPos.PrintJournal(invoiceResult, invoiceRequest, PosTypeEnumeration.Pos58mm);
        //}
        //private void PrintPosA4(InvoiceResult invoiceResult, InvoiceRequest invoiceRequest)
        //{
        //    FormatA4.PrintJournal(invoiceResult, invoiceRequest);
        //}
        private void PrintReportPos80mm(string report)
        {
            FormatPos.PrintReport(report, PosTypeEnumeration.Pos80mm);
        }
        private void PrintReportPos58mm(string report)
        {
            FormatPos.PrintReport(report, PosTypeEnumeration.Pos58mm);
        }
        //private void PrintReportPosA4(Report report)
        //{
        //    FormatA4.PrintReport(report);
        //}
        #endregion Private methods
    }
}
