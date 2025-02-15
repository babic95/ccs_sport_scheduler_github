﻿using ClosedXML.Excel;
using UniversalEsir_InputOutputExcelFiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_Database.Models;
using UniversalEsir_Settings;
using UniversalEsir_Database;
using UniversalEsir_Common.Enums;
using DocumentFormat.OpenXml.Vml;

namespace UniversalEsir_InputOutputExcelFiles
{
    public sealed class InputOutputExcelFilesManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static InputOutputExcelFilesManager instance = null;
        #endregion Fields Singleton

        #region Fields
        #endregion Fields

        #region Constructors
        private InputOutputExcelFilesManager() { }
        public static InputOutputExcelFilesManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new InputOutputExcelFilesManager();
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        public async Task<List<SupergroupDB>?> ImportSupergroups()
        {
            string? path = SettingsManager.Instance.GetPathToImportSupergroups();

            if (!string.IsNullOrEmpty(path))
            {

                List<SupergroupExcel> excelSuperoups = ImportExcel<SupergroupExcel>(path, "Nadgrupe");

                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                List<SupergroupDB> supergroups = new List<SupergroupDB>();

                excelSuperoups.ForEach(async supergroup =>
                {
                    var supergroupDBs = sqliteDbContext.Supergroups.Where(g => g.Name == supergroup.Ime).ToList();

                    if (supergroupDBs.Any())
                    {
                        SupergroupDB supergroupDB = supergroupDBs.First();
                        supergroupDB.Name = supergroup.Ime;

                        sqliteDbContext.Supergroups.Update(supergroupDB);
                        supergroups.Add(supergroupDB);
                    }
                    else
                    {
                        SupergroupDB supergroupDB = new SupergroupDB()
                        {
                            Name = supergroup.Ime,
                        };
                        await sqliteDbContext.Supergroups.AddAsync(supergroupDB);
                        supergroups.Add(supergroupDB);
                    }
                });

                await sqliteDbContext.SaveChangesAsync();
                return supergroups;
            }

            return null;
        }
        public async Task<List<ItemGroupDB>?> ImportGroups()
        {
            string? path = SettingsManager.Instance.GetPathToImportGroups();

            if (!string.IsNullOrEmpty(path))
            {

                List<GroupExcel> excelItems = ImportExcel<GroupExcel>(path, "Grupe");

                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                List<ItemGroupDB> groups = new List<ItemGroupDB>();

                excelItems.ForEach(async group =>
                {
                    SupergroupDB? supergroupDB = sqliteDbContext.Supergroups.Find(group.Nadgrupa);

                    if (supergroupDB != null)
                    {
                        var itemGroupDBs = sqliteDbContext.ItemGroups.Where(g => g.Name == group.Ime).ToList();

                        if (itemGroupDBs.Any())
                        {
                            ItemGroupDB itemGroupDB = itemGroupDBs.First();
                            itemGroupDB.Name = group.Ime;
                            itemGroupDB.IdSupergroup = group.Nadgrupa;

                            sqliteDbContext.ItemGroups.Update(itemGroupDB);
                            groups.Add(itemGroupDB);
                        }
                        else
                        {
                            ItemGroupDB itemGroupDB = new ItemGroupDB()
                            {
                                Name = group.Ime,
                                IdSupergroup = group.Nadgrupa
                            };
                            await sqliteDbContext.ItemGroups.AddAsync(itemGroupDB);
                            groups.Add(itemGroupDB);
                        }
                    }
                });

                await sqliteDbContext.SaveChangesAsync();
                return groups;
            }

            return null;
        }
        public async Task<List<ItemDB>?> ImportItems()
        {
            string? path = SettingsManager.Instance.GetPathToImportItems();

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    List<ItemExcel> excelItems = ImportExcel<ItemExcel>(path, "Proizvodi");

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    List<ItemDB> items = new List<ItemDB>();

                    excelItems.ForEach(async item =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(item.Šifra) &&
                            !string.IsNullOrEmpty(item.Naziv) &&
                            !string.IsNullOrEmpty(item.Oznaka) &&
                            !string.IsNullOrEmpty(item.JM) &&
                            item.Cena != 0)
                            {
                                string sifra = item.Šifra;

                                try
                                {
                                    int sifraInt = Convert.ToInt32(item.Šifra);

                                    sifra = sifraInt.ToString("000000");
                                }
                                catch(Exception ex) { }

                                ItemGroupDB? itemGroupDB = sqliteDbContext.ItemGroups.Find(item.Grupa);

                                if (itemGroupDB != null)
                                {
                                    ItemDB? i = sqliteDbContext.Items.Find(sifra);

                                    if (i == null)
                                    {
                                        i = new ItemDB()
                                        {
                                            Id = sifra,
                                            Barcode = item.Barkod,
                                            Label = item.Oznaka,
                                            Name = item.Naziv,
                                            Jm = item.JM,
                                            IdItemGroup = item.Grupa,
                                            SellingUnitPrice = item.Cena,
                                            InputUnitPrice = 0,
                                            TotalQuantity = item.Stanje,
                                            AlarmQuantity = item.Alarm,
                                            ItemGroupNavigation = itemGroupDB,
                                            Procurements = new List<ProcurementDB>()
                                        };
                                        await sqliteDbContext.Items.AddAsync(i);
                                    }
                                    else
                                    {
                                        i.Label = item.Oznaka;
                                        i.Barcode = item.Barkod;
                                        i.Name = item.Naziv;
                                        i.Jm = item.JM;
                                        i.IdItemGroup = item.Grupa;
                                        i.SellingUnitPrice = item.Cena;
                                        i.TotalQuantity = item.Stanje;
                                        i.AlarmQuantity = item.Alarm;
                                        i.ItemGroupNavigation = itemGroupDB;

                                        sqliteDbContext.Items.Update(i);
                                    }
                                    items.Add(i);
                                }
                                await sqliteDbContext.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            int aaa = 2;
                        }
                    });
                    sqliteDbContext.SaveChanges();
                    return items;
                }
                catch(Exception ex)
                {
                    int aaaa = 2;
                }
            }

            return null;
        }
        public async Task<List<CashierDB>?> ImportCashiers()
        {
            string? path = SettingsManager.Instance.GetPathToImportCashiers();

            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                List<CashierExcel> excelCashiers = ImportExcel<CashierExcel>(path, "Kasiri");

                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                List<CashierDB> cashiers = new List<CashierDB>();

                excelCashiers.ForEach(async cashier =>
                {
                    CashierDB? cashierDB = sqliteDbContext.Cashiers.Find(cashier.Šifra);

                    if (cashierDB != null)
                    {
                        cashierDB.Id = cashier.Šifra;
                        cashierDB.Address = cashier.Adresa;
                        cashierDB.City = cashier.Grad;
                        cashierDB.Email = cashier.Email;
                        cashierDB.Jmbg = cashier.Jmbg;
                        cashierDB.Name = cashier.Ime;
                        cashierDB.ContactNumber = cashier.Telefon;
                        cashierDB.Type = cashier.Pozicija_Radnika;

                        sqliteDbContext.Cashiers.Update(cashierDB);
                    }
                    else
                    {
                        cashierDB = new CashierDB()
                        {
                            Id = cashier.Šifra,
                            Address = cashier.Adresa,
                            City = cashier.Grad,
                            Email = cashier.Email,
                            Jmbg = cashier.Jmbg,
                            Name = cashier.Ime,
                            ContactNumber = cashier.Telefon,
                            Type = cashier.Pozicija_Radnika
                        };
                        await sqliteDbContext.Cashiers.AddAsync(cashierDB);
                    }
                    cashiers.Add(cashierDB);
                });

                await sqliteDbContext.SaveChangesAsync();
                return cashiers;
            }

            return null;
        }
        public async Task<bool> ExportItems()
        {
            try
            {
                string? path = SettingsManager.Instance.GetPathToExportItems();

                if (!string.IsNullOrEmpty(path))
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();
                    var itemsDB = sqliteDbContext.Items.ToList();

                    List<ItemExcel> items = new List<ItemExcel>();
                    itemsDB.ToList().ForEach(item =>
                    {
                        ItemExcel excelItem = new ItemExcel()
                        {
                            Šifra = item.Id,
                            Barkod = item.Barcode,
                            Cena = item.SellingUnitPrice,
                            Oznaka = item.Label,
                            JM = item.Jm,
                            Naziv = item.Name,
                            Grupa = item.IdItemGroup,
                            Stanje = item.TotalQuantity,
                            Alarm = item.AlarmQuantity.Value
                        };

                        items.Add(excelItem);
                    });

                    return ExportExcel(items, path, "Proizvodi");
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> ExportCashiers()
        {
            string? path = SettingsManager.Instance.GetPathToExportCashiers();

            if (!string.IsNullOrEmpty(path))
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();
                List<CashierDB> cashiersDB = sqliteDbContext.Cashiers.ToList();

                List<CashierExcel> cashiers = new List<CashierExcel>();
                cashiersDB.ForEach(cashier =>
                {
                    cashiers.Add(new CashierExcel()
                    {
                        Adresa = cashier.Address,
                        Email = cashier.Email,
                        Grad = cashier.City,
                        Ime = cashier.Name,
                        Jmbg = cashier.Jmbg,
                        Telefon = cashier.ContactNumber,
                        Šifra = cashier.Id,
                        Pozicija_Radnika = cashier.Type
                    });
                });

                return ExportExcel(cashiers, path, "Kasiri");
            }

            return false;
        }
        public async Task<bool> ExportReport(string path, List<Report> reports)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return ExportExcel(reports, path, "Izveštaj");
            }

            return false;
        }
        public async Task<bool> ExportGroups()
        {
            string? path = SettingsManager.Instance.GetPathToExportGroups();

            if (!string.IsNullOrEmpty(path)) 
            { 
                SqliteDbContext sqliteDbContext = new SqliteDbContext();
                List<ItemGroupDB> itemGroupDBs = sqliteDbContext.ItemGroups.ToList();

                List<GroupExcel> groups = new List<GroupExcel>();
                itemGroupDBs.ForEach(group =>
                {
                    groups.Add(new GroupExcel()
                    {
                        Ime = group.Name
                    });
                });
                return ExportExcel(groups, path, "Grupe");
            }

            return false;
        }
        #endregion Public methods

        #region Private methods
        private bool ExportExcel<T>(List<T> list, string excelFilePath, string sheetName)
        {
            bool exported = false;

            using (IXLWorkbook workbook = new XLWorkbook())
            {
                workbook.AddWorksheet(sheetName).FirstCell().InsertTable<T>(list, false);

                workbook.SaveAs(excelFilePath);
                exported = true;
            }

            return exported;
        }
        private List<T> ImportExcel<T>(string excelFilePath, string sheetName)
        {
            List<T> list = new List<T>();
            Type typeOfObject = typeof(T);

            if (File.Exists(excelFilePath))
            {
                using (IXLWorkbook workbook = new XLWorkbook(excelFilePath))
                {
                    var worksSheet = workbook.Worksheets.Where(w => w.Name == sheetName).First();
                    var properties = typeOfObject.GetProperties();

                    var columns = worksSheet.FirstRow().Cells().Select((v, i) => new { Value = v.Value, Index = i + 1 });

                    foreach (IXLRow row in worksSheet.RowsUsed().Skip(1))
                    {
                        T obj = (T)Activator.CreateInstance(typeOfObject);

                        foreach (var property in properties)
                        {
                            try
                            {
                                int colIndex = columns.SingleOrDefault(c => c.Value.ToString() == property.Name.ToString()).Index;
                                var val = row.Cell(colIndex).Value;
                                var type = property.PropertyType;

                                if(type.IsEnum)
                                {
                                    CashierTypeEnumeration cashierType;
                                    try
                                    {
                                        int cType = Convert.ToInt32(val);
                                        cashierType = (CashierTypeEnumeration)cType;
                                    }
                                    catch
                                    {
                                        cashierType = (CashierTypeEnumeration)Enum.Parse(typeof(CashierTypeEnumeration), val.ToString());
                                    }
                                    property.SetValue(obj, Convert.ChangeType(cashierType, type));
                                }
                                else
                                {
                                    if(property.Name == "Cena" &&
                                        string.IsNullOrEmpty(val.ToString()))
                                    {
                                        property.SetValue(obj, Convert.ChangeType(0, type));
                                    }
                                    else
                                    {
                                        try
                                        {
                                            property.SetValue(obj, Convert.ChangeType(val, type));
                                        }
                                        catch(Exception ex)
                                        {
                                            int aaaa = 2;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return null;
                            }
                        }

                        list.Add(obj);
                    }
                }
            }
            else
            {
                return null;
            }

            return list;
        }
        #endregion Private methods
    }
}
