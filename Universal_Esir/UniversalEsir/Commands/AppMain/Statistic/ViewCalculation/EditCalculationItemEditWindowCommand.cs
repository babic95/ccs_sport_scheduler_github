﻿using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.ViewCalculation
{
    public class EditCalculationItemEditWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public EditCalculationItemEditWindowCommand(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_currentViewModel.CalculationQuantity > 0 &&
                    _currentViewModel.CalculationPrice > 0)
            {
                if (_currentViewModel.EditQuantityWindow != null)
                {
                    _currentViewModel.EditQuantityWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("KOLIČINA I ULAZNA CENA MORAJU BITI VEĆI OD 0!",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
