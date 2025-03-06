﻿using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.ViewCalculation;
using UniversalEsir_Database;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.ViewCalculation
{
    public class OpenCalculationItemEditWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public OpenCalculationItemEditWindowCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.EditWindow != null &&
                _currentViewModel.EditWindow.IsActive)
            {
                _currentViewModel.EditWindow.Close();
            }

            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            _currentViewModel.Groups = new List<Models.Sale.GroupItems>() { new Models.Sale.GroupItems(-1, -1, "Sve grupe") };
            sqliteDbContext.Items.ToList().ForEach(x =>
            {
                Models.Sale.Item item = new Models.Sale.Item(x);

                var group = sqliteDbContext.ItemGroups.Find(x.IdItemGroup);
                if (group != null)
                {
                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                    _currentViewModel.InventoryStatusAll.Add(new Invertory(item, x.IdItemGroup, x.TotalQuantity, 0, x.AlarmQuantity == null ? -1 : x.AlarmQuantity.Value, isSirovina));
                }
            });
            _currentViewModel.SearchItems = new List<Invertory>(_currentViewModel.InventoryStatusAll);

            if (sqliteDbContext.ItemGroups != null &&
                sqliteDbContext.ItemGroups.Any())
            {
                sqliteDbContext.ItemGroups.ToList().ForEach(gropu =>
                {
                    _currentViewModel.Groups.Add(new Models.Sale.GroupItems(gropu.Id, gropu.IdSupergroup, gropu.Name));
                });
            }
            _currentViewModel.AllGroups = new ObservableCollection<Models.Sale.GroupItems>(_currentViewModel.Groups);
            _currentViewModel.CurrentGroup = _currentViewModel.AllGroups.FirstOrDefault();

            _currentViewModel.InventoryStatusCalculation = new ObservableCollection<Invertory>(_currentViewModel.InventoryStatusAll);

            _currentViewModel.EditWindow = new EditCalculationWindow(_currentViewModel);
            _currentViewModel.EditWindow.ShowDialog();
        }
    }
}