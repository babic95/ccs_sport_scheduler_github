﻿using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.InventoryStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.InventoryStatus
{
    public class OpenAddOrEditGroupItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenAddOrEditGroupItemsCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            switch (parameter.ToString().ToLower())
            {
                case "new":
                    if (_currentViewModel.AllSupergroups != null &&
                        _currentViewModel.AllSupergroups.Any())
                    {
                        _currentViewModel.CurrentSupergroup = _currentViewModel.AllSupergroups.FirstOrDefault();
                        if (_currentViewModel.CurrentSupergroup != null)
                        {
                            _currentViewModel.VisibilityAllGroupItems = Visibility.Hidden;
                            _currentViewModel.CurrentGroupItems = new GroupItems(-1, _currentViewModel.CurrentSupergroup.Id, string.Empty);

                            _currentViewModel.Window = new AddOrEditGroupItemsWindow(_currentViewModel);
                            _currentViewModel.Window.ShowDialog();
                        }
                    }
                    break;
                case "edit":
                    if (_currentViewModel.AllGroupItems != null &&
                        _currentViewModel.AllGroupItems.Any()) 
                    {
                        _currentViewModel.VisibilityAllGroupItems = Visibility.Visible;
                        _currentViewModel.CurrentGroupItems = _currentViewModel.AllGroupItems.FirstOrDefault();

                        _currentViewModel.Window = new AddOrEditGroupItemsWindow(_currentViewModel);
                        _currentViewModel.Window.ShowDialog();
                    }
                    break;
                default:
                    return;
            }
        }
    }
}