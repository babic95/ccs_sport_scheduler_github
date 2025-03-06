﻿using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.InventoryStatus
{
    public class EditNormCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public EditNormCommand(InventoryStatusViewModel currentViewModel)
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
                if (parameter is string &&
                    _currentViewModel.CurrentInventoryStatus != null &&
                    _currentViewModel.CurrentInventoryStatus.Item != null)
                {
                    string idItem = (string)parameter;

                    var result = MessageBox.Show("Da li zaista želite da izmenite normativ artikala?",
                        "Izmena normativa artikala",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        ItemDB? currentItemDB = sqliteDbContext.Items.Find(_currentViewModel.CurrentInventoryStatus.Item.Id);

                        if (currentItemDB != null)
                        {
                            var itemInNorm = sqliteDbContext.ItemsInNorm.FirstOrDefault(x => x.IdNorm == currentItemDB.IdNorm && x.IdItem == idItem);

                            if (itemInNorm != null)
                            {
                                _currentViewModel.NormQuantity = itemInNorm.Quantity;
                                _currentViewModel.QuantityCommandParameter = "QuantityEdit";

                                _currentViewModel.WindowHelper = new AddQuantityToNormWindow(_currentViewModel);
                                _currentViewModel.WindowHelper.ShowDialog();

                                var norm = _currentViewModel.Norma.FirstOrDefault(norm => norm.Item.Id == idItem);

                                if (norm != null)
                                {
                                    if (_currentViewModel.NormQuantity > 0)
                                    {
                                        norm.Quantity = _currentViewModel.NormQuantity;
                                        itemInNorm.Quantity = _currentViewModel.NormQuantity;

                                        sqliteDbContext.ItemsInNorm.Update(itemInNorm);
                                        sqliteDbContext.SaveChanges();
                                        _currentViewModel.NormQuantity = 0;
                                    }
                                    else
                                    {
                                        MessageBox.Show("KOLIČINA MORA BITI BROJ!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja normativa iz artikla!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}