using UniversalEsir.Enums.AppMain.Statistic;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Common.Enums;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Nivelacija
{
    public class SaveNivelacijaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NivelacijaViewModel _currentViewModel;

        public SaveNivelacijaCommand(NivelacijaViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da kreirate nivelaciju?",
                           "Kreiranje nivelacije",
                           MessageBoxButton.YesNo,
                           MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool onlySirovine = true;
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    decimal totalNivelacija = 0;

                    NivelacijaDB nivelacijaDB = new NivelacijaDB()
                    {
                        Id = _currentViewModel.CurrentNivelacija.Id,
                        Counter = _currentViewModel.CurrentNivelacija.CounterNivelacije,
                        DateNivelacije = _currentViewModel.CurrentNivelacija.NivelacijaDate,
                        Description = _currentViewModel.CurrentNivelacija.Description,
                        Type = (int)_currentViewModel.CurrentNivelacija.Type
                    };

                    sqliteDbContext.Nivelacijas.Add(nivelacijaDB);
                    sqliteDbContext.SaveChanges();

                    _currentViewModel.CurrentNivelacija.NivelacijaItems.ToList().ForEach(item =>
                    {
                        var itemDB = sqliteDbContext.Items.Find(item.IdItem);
                        if (itemDB != null)
                        {
                            var group = sqliteDbContext.ItemGroups.Find(itemDB.IdItemGroup);

                            if (group == null)
                            {
                                MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                                return;
                            }
                            if (group.Name.ToLower().Contains("sirovine") ||
                                group.Name.ToLower().Contains("sirovina"))
                            {
                            }
                            else
                            {
                                onlySirovine = false;
                                ItemNivelacijaDB itemNivelacijaDB = new ItemNivelacijaDB()
                                {
                                    IdNivelacija = _currentViewModel.CurrentNivelacija.Id,
                                    IdItem = item.IdItem,
                                    NewUnitPrice = item.NewPrice,
                                    OldUnitPrice = item.OldPrice,
                                    StopaPDV = item.StopaPDV,
                                    TotalQuantity = item.Quantity,
                                };
                                sqliteDbContext.ItemsNivelacija.Add(itemNivelacijaDB);

                                itemDB.SellingUnitPrice = item.NewPrice;
                                sqliteDbContext.Items.Update(itemDB);

                                totalNivelacija += (itemNivelacijaDB.NewUnitPrice - itemNivelacijaDB.OldUnitPrice) * itemNivelacijaDB.TotalQuantity;
                            }
                        }
                    });

                    if (onlySirovine)
                    {
                        MessageBox.Show("Nivelacija ne može da se radi samo na SIROVINE!", "Nivelacija sirovina!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        sqliteDbContext.Nivelacijas.Remove(nivelacijaDB);
                        sqliteDbContext.SaveChanges();
                    }
                    else
                    {
                        sqliteDbContext.SaveChanges();

                        KepDB kepDB = new KepDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            KepDate = nivelacijaDB.DateNivelacije,
                            Type = (int)KepStateEnumeration.Nivelacija,
                            Razduzenje = 0,
                            Zaduzenje = totalNivelacija,
                            Description = $"Ručna nivelacija 'Nivelacija_{nivelacijaDB.Counter}-{nivelacijaDB.DateNivelacije.Year}'"
                        };
                        sqliteDbContext.Kep.Add(kepDB);
                        sqliteDbContext.SaveChanges();

                        Log.Debug($"SaveNivelacijaCommand - Uspesno sacuvana nivelacija {_currentViewModel.CurrentNivelacija.Id}");
                        MessageBox.Show("Uspešno ste kreirali nivelaciju!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    _currentViewModel.Reset();
                }
                catch(Exception ex)
                {
                    Log.Error("SaveNivelacijaCommand - Greska prilikom cuvanja nivelacije u bazu -> ", ex);
                }
            }
        }
    }
}