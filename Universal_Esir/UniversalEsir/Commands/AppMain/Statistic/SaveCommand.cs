using UniversalEsir.Enums.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic.Knjizenje;
using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Nivelacija;
using UniversalEsir_Common.Models.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Printer.PaperFormat;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic
{
    public class SaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public SaveCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(_currentViewModel is AddEditSupplierViewModel)
            {
                AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;
                if (addEditSupplierViewModel.CurrentSupplier.Id == null ||
                    addEditSupplierViewModel.CurrentSupplier.Id < 1)
                {
                    AddEditSupplier();
                }
                else
                {
                    AddEditSupplier(addEditSupplierViewModel.CurrentSupplier.Id);
                }
            }
            else if(_currentViewModel is InventoryStatusViewModel)
            {
                InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

                if (inventoryStatusViewModel.CurrentInventoryStatus == null ||
                    !inventoryStatusViewModel.ItemForEdit)
                {
                    AddEditItem();
                }
                else
                {
                    AddEditItem(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id);
                }
            }
        }
        
        
        private void AddEditSupplier(int? id = null)
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();
            AddEditSupplierViewModel addEditSupplierViewModel = (AddEditSupplierViewModel)_currentViewModel;

            if (id == null)
            {
                SupplierDB supplier = new SupplierDB();
                try
                {
                    supplier.Name = addEditSupplierViewModel.CurrentSupplier.Name;
                    supplier.Pib = addEditSupplierViewModel.CurrentSupplier.Pib;
                    supplier.Mb = addEditSupplierViewModel.CurrentSupplier.MB;
                    supplier.Address = addEditSupplierViewModel.CurrentSupplier.Address;
                    supplier.City = addEditSupplierViewModel.CurrentSupplier.City;
                    supplier.ContractNumber = addEditSupplierViewModel.CurrentSupplier.ContractNumber;
                    supplier.Email = addEditSupplierViewModel.CurrentSupplier.Email;

                    sqliteDbContext.Add(supplier);
                    sqliteDbContext.SaveChanges();

                    MessageBox.Show("Uspešno ste dodali dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                    addEditSupplierViewModel.Window.Close();
                }
                catch
                {
                    MessageBox.Show("Greška prilikom dodavanja dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite dobavljača?", "",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var supplier = sqliteDbContext.Suppliers.Find(id);

                        if (supplier != null)
                        {
                            supplier.Name = addEditSupplierViewModel.CurrentSupplier.Name;
                            supplier.Pib = addEditSupplierViewModel.CurrentSupplier.Pib;
                            supplier.Mb = addEditSupplierViewModel.CurrentSupplier.MB;
                            supplier.Address = addEditSupplierViewModel.CurrentSupplier.Address;
                            supplier.City = addEditSupplierViewModel.CurrentSupplier.City;
                            supplier.ContractNumber = addEditSupplierViewModel.CurrentSupplier.ContractNumber;
                            supplier.Email = addEditSupplierViewModel.CurrentSupplier.Email;

                            sqliteDbContext.Suppliers.Update(supplier);
                            sqliteDbContext.SaveChanges();

                            MessageBox.Show("Uspešno ste izmenili dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                            addEditSupplierViewModel.Window.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ne postoji dobavljač!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom izmene dobavljača!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            addEditSupplierViewModel.SuppliersAll = new List<Supplier>();
            sqliteDbContext.Suppliers.ToList().ForEach(x =>
            {
                addEditSupplierViewModel.SuppliersAll.Add(new Supplier(x));
            });

            addEditSupplierViewModel.Suppliers = new ObservableCollection<Supplier>(addEditSupplierViewModel.SuppliersAll);
            addEditSupplierViewModel.CurrentSupplier = new Supplier();
        }
    
        private void AddEditItem(string? id = null)
        {
            InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;

            if (inventoryStatusViewModel.CurrentGroupItems == null)
            {
                MessageBox.Show("Artikal mora da pripada nekoj grupi!", "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (string.IsNullOrEmpty(id)) 
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da dodate artikal?", "Dodavanje artikla",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        if (sqliteDbContext.Items != null && 
                            sqliteDbContext.Items.Any())
                        {
                            if(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id.Length > 6)
                            {
                                Log.Error("SaveCommand -> AddEditItem -> predugacka sifra");
                                MessageBox.Show("Artikal može da ima maximalno 6 karaktera!",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }

                            var item = sqliteDbContext.Items.Find(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id.PadLeft(6, '0'));

                            if(item != null)
                            {
                                Log.Error("SaveCommand -> AddEditItem -> Vec postoji artikal sa zadatom sifrom");
                                MessageBox.Show("Već postoji artikal unetom šifrom!",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }
                        }

                        ItemDB itemDB = new ItemDB()
                        {
                            Id = inventoryStatusViewModel.CurrentInventoryStatus.Item.Id.PadLeft(6, '0'),
                            Name = inventoryStatusViewModel.CurrentInventoryStatus.Item.Name,
                            InputUnitPrice = 0,
                            SellingUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingUnitPrice,
                            Label = inventoryStatusViewModel.CurrentLabel.Id,
                            Jm = inventoryStatusViewModel.CurrentInventoryStatus.Item.Jm,
                            AlarmQuantity = inventoryStatusViewModel.CurrentInventoryStatus.Alarm,
                            TotalQuantity = 0,
                            IdItemGroup = inventoryStatusViewModel.CurrentGroupItems.Id,
                            IdNorm = inventoryStatusViewModel.CurrentNorm > 0 ? inventoryStatusViewModel.CurrentNorm : null
                        };

                        if (sqliteDbContext.Items != null)
                        {
                            sqliteDbContext.Items.Add(itemDB);
                            sqliteDbContext.SaveChanges();

                            if (inventoryStatusViewModel.Norma != null && inventoryStatusViewModel.Norma.Any())
                            {
                                inventoryStatusViewModel.Norma.ToList().ForEach(item =>
                                {
                                    var norms = sqliteDbContext.ItemsInNorm.Where(it => it.IdNorm == itemDB.IdNorm && it.IdItem == item.Item.Id);

                                    if (norms.Any())
                                    {
                                        ItemInNormDB? itemInNormDB = norms.FirstOrDefault();

                                        if (itemInNormDB != null)
                                        {
                                            itemInNormDB.Quantity = item.Quantity;

                                            sqliteDbContext.Update(itemInNormDB);
                                            sqliteDbContext.SaveChangesAsync();
                                        }
                                    }
                                    else
                                    {
                                        ItemInNormDB itemInNormDB = new ItemInNormDB()
                                        {
                                            IdItem = item.Item.Id,
                                            IdNorm = itemDB.IdNorm.Value,
                                            Quantity = item.Quantity,
                                        };

                                        sqliteDbContext.ItemsInNorm.AddAsync(itemInNormDB);
                                        sqliteDbContext.SaveChangesAsync();
                                    }
                                });

                                sqliteDbContext.Items.Update(itemDB);
                                sqliteDbContext.SaveChanges();
                            }

                            inventoryStatusViewModel.InventoryStatusAll = new List<Invertory>();
                            sqliteDbContext.Items.ToList().ForEach(x =>
                            {
                                Item item = new Item(x);

                                var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);

                                if (group != null)
                                {
                                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                                    inventoryStatusViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity == null ? -1 : x.AlarmQuantity.Value, isSirovina));
                                }
                                else
                                {
                                    MessageBox.Show("Artikal ne pripada ni jednoj grupi!!!",
                                        "Greška",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                                    return;
                                }
                            });
                            inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                            inventoryStatusViewModel.InventoryStatusNorm = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                            inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                            inventoryStatusViewModel.NormQuantity = 0;
                            inventoryStatusViewModel.VisibilityNext = Visibility.Hidden;
                            inventoryStatusViewModel.SearchItems = string.Empty;
                            inventoryStatusViewModel.CurrentInventoryStatusNorm = null;
                            if (inventoryStatusViewModel.WindowHelper != null)
                            {
                                inventoryStatusViewModel.WindowHelper.Close();
                            }

                            MessageBox.Show("Uspešno ste dodali artikal!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                            inventoryStatusViewModel.Window.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Greška prilikom dodavanja novog artikla!", "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                } 
            }
            else
            {
                var result = MessageBox.Show("Da li ste sigurni da želite da izmenite artikal?", "Izmena artikla",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        var itemDB = sqliteDbContext.Items.Find(inventoryStatusViewModel.CurrentInventoryStatus.Item.Id);

                        if (itemDB == null)
                        {
                            MessageBox.Show("GREŠKA U IZMENI ARTIKLA!", "", MessageBoxButton.OK, MessageBoxImage.Error);

                            return;
                        }

                        if (inventoryStatusViewModel.CurrentNorm >= 0)
                        {
                            itemDB.IdNorm = inventoryStatusViewModel.CurrentNorm;
                        }

                        itemDB.Barcode = inventoryStatusViewModel.CurrentInventoryStatus.Item.Barcode;
                        itemDB.Name = inventoryStatusViewModel.CurrentInventoryStatus.Item.Name;
                        itemDB.SellingUnitPrice = inventoryStatusViewModel.CurrentInventoryStatus.Item.SellingUnitPrice;
                        itemDB.Label = inventoryStatusViewModel.CurrentLabel.Id;
                        itemDB.Jm = inventoryStatusViewModel.CurrentInventoryStatus.Item.Jm;
                        itemDB.AlarmQuantity = inventoryStatusViewModel.CurrentInventoryStatus.Alarm;
                        itemDB.IdItemGroup = inventoryStatusViewModel.CurrentGroupItems.Id;

                        inventoryStatusViewModel.Norma.ToList().ForEach(item =>
                        {
                            var norms = sqliteDbContext.ItemsInNorm.Where(it => it.IdNorm == itemDB.IdNorm && it.IdItem == item.Item.Id);

                            if (norms.Any())
                            {
                                ItemInNormDB? itemInNormDB = norms.FirstOrDefault();

                                if (itemInNormDB != null)
                                {
                                    itemInNormDB.Quantity = item.Quantity;

                                    sqliteDbContext.Update(itemInNormDB);
                                    sqliteDbContext.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                ItemInNormDB itemInNormDB = new ItemInNormDB()
                                {
                                    IdItem = item.Item.Id,
                                    IdNorm = itemDB.IdNorm.Value,
                                    Quantity = item.Quantity,
                                };

                                sqliteDbContext.ItemsInNorm.AddAsync(itemInNormDB);
                                sqliteDbContext.SaveChangesAsync();
                            }
                        });

                        inventoryStatusViewModel.InventoryStatusAll = new List<Invertory>();
                        sqliteDbContext.Items.ToList().ForEach(x =>
                        {
                            Item item = new Item(x);

                            var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);

                            if(group != null)
                            {
                                bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                inventoryStatusViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity == null ? -1 : x.AlarmQuantity.Value, isSirovina));
                            }
                            else
                            {
                                MessageBox.Show("Artikal ne pripada ni jednoj grupi!!!",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                                return;
                            }
                        });
                        inventoryStatusViewModel.InventoryStatus = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                        inventoryStatusViewModel.InventoryStatusNorm = new ObservableCollection<Invertory>(inventoryStatusViewModel.InventoryStatusAll);
                        inventoryStatusViewModel.Norma = new ObservableCollection<Invertory>();
                        inventoryStatusViewModel.NormQuantity = 0;
                        inventoryStatusViewModel.VisibilityNext = Visibility.Hidden;
                        inventoryStatusViewModel.SearchItems = string.Empty;
                        inventoryStatusViewModel.CurrentInventoryStatusNorm = null;
                        if (inventoryStatusViewModel.WindowHelper != null)
                        {
                            inventoryStatusViewModel.WindowHelper.Close();
                        }

                        sqliteDbContext.Items.Update(itemDB);
                        sqliteDbContext.SaveChanges();

                        MessageBox.Show("Uspešno ste izmenili artikal!", "", MessageBoxButton.OK, MessageBoxImage.Information);

                        inventoryStatusViewModel.Window.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Greška prilikom izmene artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}