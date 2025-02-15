﻿using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Printer.Models;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Printer.PaperFormat
{
    internal class VirmanInvoice
    {
        #region Fields
        private static MorePage? _morePage;

        private static string _startFiscal;
        private static string _endFiscal;
        private static string _taxpayerInformationFix;
        private static string _customerDataInformationFix;
        private static string _journalItemsFix;
        private static string _journalItems;

        private static string _typeFiscal;

        private static readonly float _fontSizeInMM = 3.12f;
        private static Otpremnica _otpremnica;
        private static int _width;
        #endregion Fields

        #region Constructors
        #endregion Constructors

        #region Public methods
        public static void PrintJournal(Otpremnica otpremnica)
        {
            try
            {
                _otpremnica = otpremnica;

                _journalItemsFix = "                                                                                                    \r\n";
                _journalItemsFix += "                                                                                                    \r\n";
                _journalItemsFix += "Artikli\r\n";
                _journalItemsFix += "   \r\n";
                _journalItemsFix += string.Format("{0}{1}{2}{3}{4}\r\n", "Naziv".PadRight(25), "JM".PadRight(10), "Cena".PadRight(20), "Kol.".PadRight(14), "Ukupno".PadLeft(14));
                _journalItemsFix += "                                                                                                    \r\n";
                _journalItemsFix += "                                                                                                    \r\n";

                _journalItems = GetJournalItems();
                _journalItemsFix += SplitInParts(string.Format("{0:#,##0.00}", Decimal.Round(_otpremnica.TotalAmount, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.'), "Ukupan iznos:", 30);

                CreateVirmanJournal();
                //_taxpayerInformationFix = GetTaxpayerInformationForJournal();
                _customerDataInformationFix = GetCustomerDataInformationForJournal();

                string? prName = SettingsManager.Instance.GetPrinterName();

                if (!string.IsNullOrEmpty(prName))
                {
                    var pdoc = new PrintDocument();
                    PrinterSettings ps = new PrinterSettings();
                    pdoc.PrinterSettings.PrinterName = ps.PrinterName;
                    _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                    _morePage = null;
                    pdoc.PrintPage += new PrintPageEventHandler(dailyDep);
                    pdoc.Print();
                    pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
                    _morePage = null;
                }
            }
            catch { }
        }
        public static void PrintPonuda(Otpremnica otpremnica)
        {
            try
            {
                _otpremnica = otpremnica;

                _journalItemsFix = "                                                                                                    \r\n";
                _journalItemsFix += "                                                                                                    \r\n";
                _journalItemsFix += "Artikli\r\n";
                _journalItemsFix += "   \r\n";
                _journalItemsFix += string.Format("{0}{1}{2}{3}{4}\r\n", "Naziv".PadRight(25), "JM".PadRight(10), "Cena".PadRight(20), "Kol.".PadRight(14), "Ukupno".PadLeft(14));
                _journalItemsFix += "                                                                                                    \r\n";
                _journalItemsFix += "                                                                                                    \r\n";

                _journalItems = GetJournalItems();
                _journalItemsFix += SplitInParts(string.Format("{0:#,##0.00}", Decimal.Round(_otpremnica.TotalAmount, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.'), "Ukupan iznos:", 30);

                CreateVirmanJournal();
                //_taxpayerInformationFix = GetTaxpayerInformationForJournal();
                _customerDataInformationFix = GetCustomerDataInformationForJournal();

                var pdoc = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                //pdoc.PrinterSettings.PrinterName = ps.PrinterName;
                _width = pdoc.PrinterSettings.DefaultPageSettings.PaperSize.Width;

                _morePage = null;
                pdoc.PrintPage += new PrintPageEventHandler(dailyDep);
                pdoc.Print();
                pdoc.PrintPage -= new PrintPageEventHandler(dailyDep);
                _morePage = null;
            }
            catch { }
        }
        #endregion Public methods

        #region Private methods
        private static void dailyDep(object sender, PrintPageEventArgs e)
        {
            try
            {
                const float neededHeight = 773.7007874F;
                string newRow = "                                                                                                    \r\n";
                string line = "----------------------------------------------------------------------------------------------------\r\n";
                Graphics graphics = e.Graphics;
                graphics.PageUnit = GraphicsUnit.Point;
                Font drawFontRegular = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontRegularBold = new Font("Cascadia Code",
                    _fontSizeInMM,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                Font drawFontBiger1 = new Font("Cascadia Code",
                    _fontSizeInMM * 3,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger2 = new Font("Cascadia Code",
                    _fontSizeInMM * 2,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger3 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.5f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);
                Font drawFontBiger4 = new Font("Cascadia Code",
                    _fontSizeInMM * 1.2f,
                    FontStyle.Regular, GraphicsUnit.Millimeter);

                SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                SolidBrush drawBrushWhite = new SolidBrush(System.Drawing.Color.White);
                SolidBrush drawBrushGray = new SolidBrush(System.Drawing.Color.Gray);
                //string[] splitForPrintTaxpayerInformationFix = _taxpayerInformationFix.Split("\r\n");
                string[] splitForPrintCustomerDataInformationFix = _customerDataInformationFix.Split("\r\n");
                string[] splitForPrintItemsFix = _journalItemsFix.Split("\r\n");
                string[] splitForPrintItems = _journalItems.Split("\r\n");
                string[] splitForPrintStart = _startFiscal.Split("\r\n");
                string[] splitForPrintEnd = _endFiscal.Split("\r\n");

                List<string> signature = new List<string>();

                //float xL = 18.346456693F;
                float xL = 30F;
                float xR = 0;
                float yL = 18.346456693F;
                float yR = 28.346456693F;
                float width = 0; // max width I found through trial and error
                float height = 0F;

                width = graphics.MeasureString(line, drawFontRegular).Width;
                height = graphics.MeasureString(line, drawFontRegular).Height;

                xR = width / 2 + xL + xL - 4;

                string strBLOBFilePath = SettingsManager.Instance.GetPathToLogo();

                if (_morePage is null)
                {
                    if (!string.IsNullOrEmpty(strBLOBFilePath) &&
                        File.Exists(strBLOBFilePath))
                    {
                        FileStream fsBLOBFile = new FileStream(strBLOBFilePath, FileMode.Open, FileAccess.Read);
                        Byte[] bytBLOBData = new Byte[fsBLOBFile.Length];
                        fsBLOBFile.Read(bytBLOBData, 0, bytBLOBData.Length);
                        fsBLOBFile.Close();
                        using (MemoryStream ms = new MemoryStream(bytBLOBData))
                        {
                            var img = System.Drawing.Image.FromStream(ms);

                            var size = width * 0.10F;
                            var xx = xL + width * 0.45F;
                            graphics.DrawImage(img, new RectangleF(xx, 0 + height, size, size));
                            yL = size + 2 * height;
                            yR = size + 2 * height;
                        }
                    }

                    for (int i = 0; i < splitForPrintStart.Length - 1; i++)
                    {
                        if (i == 0)
                        {
                            graphics.DrawString(splitForPrintStart[i], drawFontBiger1, drawBrush, xL, yL);
                            yL += graphics.MeasureString(splitForPrintStart[i], drawFontBiger1).Height;
                        }
                        else
                        {
                            graphics.DrawString(splitForPrintStart[i], drawFontRegularBold, drawBrush, xR, yL);
                            yL += graphics.MeasureString(splitForPrintStart[i], drawFontRegularBold).Height;
                        }
                    }

                    graphics.DrawString(newRow, drawFontRegular, drawBrush, xL, yL);
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;
                    graphics.DrawString(line, drawFontRegular, drawBrush, xL, yL);
                    yL += graphics.MeasureString(line, drawFontRegular).Height;
                    graphics.DrawString(newRow, drawFontRegular, drawBrush, xL, yL);
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    yR = yL;

                    //graphics.DrawString("Prodavac:", drawFontBiger2, drawBrush, xL, yL);
                    //yL += graphics.MeasureString("Prodavac:", drawFontBiger2).Height;
                    //graphics.DrawString(newRow, drawFontRegular, drawBrush, xL, yL);
                    //yL += graphics.MeasureString(newRow, drawFontRegular).Height;

                    if (splitForPrintCustomerDataInformationFix.Length > 0)
                    {
                        graphics.DrawString("Kupac:", drawFontBiger2, drawBrush, xR, yR);
                        yR += graphics.MeasureString("Kupac:", drawFontBiger2).Height;
                        graphics.DrawString(newRow, drawFontRegular, drawBrush, xL, yL);
                        yR += graphics.MeasureString(newRow, drawFontRegular).Height;
                    }

                    //for (int i = 0; i < splitForPrintTaxpayerInformationFix.Length - 1; i++)
                    //{
                    //    graphics.DrawString(splitForPrintTaxpayerInformationFix[i], drawFontRegular, drawBrush, xL, yL);
                    //    yL += graphics.MeasureString(splitForPrintTaxpayerInformationFix[i], drawFontRegular).Height;
                    //}

                    for (int i = 0; i < splitForPrintCustomerDataInformationFix.Length; i++)
                    {
                        graphics.DrawString(splitForPrintCustomerDataInformationFix[i], drawFontRegular, drawBrush, xR, yR);
                        yR += graphics.MeasureString(splitForPrintCustomerDataInformationFix[i], drawFontRegular).Height;
                    }

                    graphics.DrawString(newRow, drawFontRegular, drawBrush, xL, yL);
                    yL += graphics.MeasureString(newRow, drawFontRegular).Height;
                }

                float y = yR > yL ? yR : yL;

                int ind = 0;

                if (_morePage is not null && _morePage.Type == Models.Type.Items)
                {
                    ind = splitForPrintItemsFix.Length - 2;
                }
                else if (_morePage is not null && _morePage.Type == Models.Type.ItemsFix)
                {
                    ind = _morePage.Index;
                }
                else if (_morePage is not null)
                {
                    ind = -1;
                }
                if (ind >= 0)
                {
                    for (; ind < splitForPrintItemsFix.Length - 1; ind++)
                    {
                        if (y < neededHeight)
                        {
                            if (ind == splitForPrintItemsFix.Length - 2)
                            {
                                int j = 0;
                                if (_morePage is not null && _morePage.Type == Models.Type.Items)
                                {
                                    j = _morePage.Index;
                                }
                                for (; j < splitForPrintItems.Length - 1; j++)
                                {
                                    if (y < neededHeight)
                                    {
                                        graphics.DrawString(splitForPrintItems[j], drawFontRegular, drawBrush, xL, y);
                                        y += graphics.MeasureString(splitForPrintItems[j], drawFontRegular).Height;
                                    }
                                    else
                                    {
                                        e.HasMorePages = true;
                                        _morePage = new MorePage()
                                        {
                                            Type = Models.Type.Items,
                                            Index = j
                                        };
                                        return;
                                    }
                                }
                            }

                            if (splitForPrintItemsFix[ind].ToLower().Contains("naziv"))
                            {
                                var currentY = graphics.MeasureString(splitForPrintItemsFix[ind], drawFontRegularBold).Height;
                                var rect = new RectangleF(xL, y - currentY, width,
                                    graphics.MeasureString(splitForPrintItemsFix[ind], drawFontRegular).Height + 2 * currentY);
                                graphics.FillRectangle(drawBrushGray, rect);
                            }

                            if (splitForPrintItemsFix[ind].ToLower().Contains("artikli"))
                            {
                                graphics.DrawString(splitForPrintItemsFix[ind], drawFontBiger2, drawBrush, xL, y);
                                y += graphics.MeasureString(splitForPrintItemsFix[ind], drawFontBiger2).Height;
                            }
                            else
                            {
                                if (splitForPrintItemsFix[ind].ToLower().Contains("ukupan iznos"))
                                {
                                    graphics.DrawString(splitForPrintItemsFix[ind], drawFontBiger3, drawBrush, xR, y);
                                    y += graphics.MeasureString(splitForPrintItemsFix[ind], drawFontBiger3).Height;
                                }
                                else
                                {
                                    if (splitForPrintItemsFix[ind].ToLower().Contains("naziv"))
                                    {
                                        graphics.DrawString(splitForPrintItemsFix[ind], drawFontBiger4, drawBrush, xL, y);
                                        y += graphics.MeasureString(splitForPrintItemsFix[ind], drawFontBiger4).Height;
                                    }
                                    else
                                    {
                                        graphics.DrawString(splitForPrintItemsFix[ind], drawFontRegularBold, drawBrush, xL, y);
                                        y += graphics.MeasureString(splitForPrintItemsFix[ind], drawFontRegular).Height;
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.HasMorePages = true;
                            _morePage = new MorePage()
                            {
                                Type = Models.Type.ItemsFix,
                                Index = ind
                            };
                            return;
                        }
                    }
                }

                if (y + graphics.MeasureString(newRow, drawFontRegular).Height < neededHeight)
                {
                    graphics.DrawString(newRow, drawFontRegular, drawBrush, xL, y);
                    y += graphics.MeasureString(newRow, drawFontRegular).Height;
                }
                else
                {
                    e.HasMorePages = true;
                    _morePage = new MorePage()
                    {
                        Type = Models.Type.End,
                        Index = 0
                    };
                    return;
                }

                if (y + 7 * graphics.MeasureString(newRow, drawFontRegular).Height < neededHeight)
                {
                    y += 5 * graphics.MeasureString(newRow, drawFontRegular).Height;

                    graphics.DrawString(splitForPrintEnd[0], drawFontRegular, drawBrush, xL, y);
                    graphics.DrawString(splitForPrintEnd[1], drawFontRegular, drawBrush, xR, y);

                    y += 2 * graphics.MeasureString(splitForPrintEnd[0], drawFontRegular).Height;
                }
                else
                {
                    e.HasMorePages = true;
                    _morePage = new MorePage()
                    {
                        Type = Models.Type.Copy,
                        Index = 0
                    };
                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void CreateVirmanJournal()
        {
            _startFiscal = $"{_otpremnica.InvoiceNumberResult}\r\n";
            if (!string.IsNullOrEmpty(_otpremnica.Porudzbenica))
            {
                _startFiscal += SplitInParts(_otpremnica.Porudzbenica, $"Porudzbenica: ", 45);
            }
            _startFiscal += SplitInParts(_otpremnica.SdcDateTime.ToString("dd.MM.yyyy HH:mm:ss"), $"Vreme: ", 45);

            _endFiscal = "Potpis izdavaoca:______________________________\r\n";
            _endFiscal += "Potpis kupca:______________________________";
        }
        //private static string GetTaxpayerInformationForJournal()
        //{
            //string taxpayerInformation = string.Empty;
            //if (!string.IsNullOrEmpty(_otpremnica.Tin))
            //{
            //    taxpayerInformation += SplitInParts(_otpremnica.Tin, "PIB: ", 45);
            //    taxpayerInformation += "---------------------------------------------\r\n";
            //}
            //if (!string.IsNullOrEmpty(_otpremnica.MB))
            //{
            //    taxpayerInformation += SplitInParts(_otpremnica.MB, "MB: ", 45);
            //    taxpayerInformation += "---------------------------------------------\r\n";
            //}
            //if (!string.IsNullOrEmpty(_otpremnica.BusinessName))
            //{
            //    taxpayerInformation += SplitInParts(_otpremnica.BusinessName, "NAZIV: ", 45);
            //    taxpayerInformation += "---------------------------------------------\r\n";
            //}
            ////taxpayerInformation += SplitInParts(_invoiceResult.LocationName, "", 45);
            //if (!string.IsNullOrEmpty(_otpremnica.Address))
            //{
            //    taxpayerInformation += SplitInParts(_otpremnica.Address, "ADRESA: ", 45);
            //    taxpayerInformation += "---------------------------------------------\r\n";
            //}
            //if (!string.IsNullOrEmpty(_otpremnica.BankAccount))
            //{
            //    taxpayerInformation += SplitInParts(_otpremnica.BankAccount, $"BANKA: ", 45);
            //    taxpayerInformation += "---------------------------------------------\r\n";
            //}

            //return taxpayerInformation;
        //}
        private static string GetCustomerDataInformationForJournal()
        {
            string posInformation = string.Empty;

            if (!string.IsNullOrEmpty(_otpremnica.BuyerId))
            {
                var splitBuyerId = _otpremnica.BuyerId.Split(":");
                posInformation += SplitInParts(splitBuyerId[1].Trim(), "PIB: ", 45);
                posInformation += "---------------------------------------------\r\n";
            }

            if (!string.IsNullOrEmpty(_otpremnica.BuyerName))
            {
                posInformation += SplitInParts(_otpremnica.BuyerName, "Naziv kupca: ", 45);
                posInformation += "---------------------------------------------\r\n";
            }

            if (!string.IsNullOrEmpty(_otpremnica.BuyerAddress))
            {
                posInformation += SplitInParts(_otpremnica.BuyerAddress, "Adresa kupca: ", 45);
                posInformation += "---------------------------------------------\r\n";
            }

            return posInformation;
        }
        private static string GetJournalItems()
        {
            string result = string.Empty;

            foreach (Item item in _otpremnica.Items)
            {
                string i = item.Name;

                string name = SplitInParts(i, "", 30, 1);

                string[] splitName = name.Split("\r\n");

                if (splitName.Length > 1)
                {
                    name = string.Empty;
                    int length = splitName.Length;
                    if (splitName[splitName.Length - 1].Length == 0)
                    {
                        length = splitName.Length - 1;
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (j == length - 1)
                        {
                            name += $"{splitName[j]}";
                        }
                        else
                        {
                            name += $"{splitName[j]}\r\n";
                        }
                    }
                }

                decimal price = item.TotalAmount / item.Quantity;
                string jm = $"{item.Jm}".PadRight(12);
                string unitPrice = string.Format("{0:#,##0.00}", Decimal.Round(price, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadRight(23);
                string quantity = string.Format("{0:#,##0.000}", Decimal.Round(item.Quantity, 3)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadRight(15);
                string totalAmount = string.Format("{0:#,##0.00}", Decimal.Round(item.TotalAmount, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.').PadLeft(20);
                result += $"{name}{jm}{unitPrice}{quantity}{totalAmount}\r\n";

                result += "----------------------------------------------------------------------------------------------------\r\n";
            }

            return result;
        }
        private static string CenterString(string value, int length, bool newRow = true)
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

                if (newRow)
                {
                    return $"{value.PadLeft(padLeft).PadRight(length)}\r\n";
                }
                else
                {
                    return $"{value.PadLeft(padLeft).PadRight(length)}";
                }
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
                    journal = string.Format("{0}{1}", fixedPart, value.PadRight(length));
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
        #endregion Private methods
    }
}
