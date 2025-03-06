﻿using UniversalEsir.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.PriceIncrease
{
    public class LowerPricesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PriceIncreaseViewModel _currentViewModel;

        public LowerPricesCommand(PriceIncreaseViewModel currentViewModel)
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
                decimal price = Convert.ToDecimal(parameter);
                _currentViewModel.Total -= price;
            }
            catch
            {
                MessageBox.Show("Greška u parametru!\nPozovite proizvodjaca!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}