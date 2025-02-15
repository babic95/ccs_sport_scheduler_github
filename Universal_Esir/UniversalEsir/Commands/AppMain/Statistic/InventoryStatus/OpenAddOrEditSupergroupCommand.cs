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
    public class OpenAddOrEditSupergroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenAddOrEditSupergroupCommand(InventoryStatusViewModel currentViewModel)
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
                    _currentViewModel.VisibilityAllSupergroup = Visibility.Hidden;
                    _currentViewModel.CurrentSupergroup = new Supergroup(-1, string.Empty);

                    _currentViewModel.Window = new AddOrEditSupergroupWindow(_currentViewModel);
                    _currentViewModel.Window.ShowDialog();
                    break;
                case "edit":
                    if(_currentViewModel.AllSupergroups != null &&
                        _currentViewModel.AllSupergroups.Any())
                    {
                        _currentViewModel.VisibilityAllSupergroup = Visibility.Visible;
                        _currentViewModel.CurrentSupergroup = _currentViewModel.AllSupergroups.FirstOrDefault();

                        _currentViewModel.Window = new AddOrEditSupergroupWindow(_currentViewModel);
                        _currentViewModel.Window.ShowDialog();
                    }
                    break;
                default:
                    return;
            }
        }
    }
}