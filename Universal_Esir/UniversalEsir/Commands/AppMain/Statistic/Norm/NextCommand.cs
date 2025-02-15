using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Norm
{
    public class NextCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewModelBase _currentViewModel;

        public NextCommand(ViewModelBase currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(parameter is string)
            {
                if (_currentViewModel is InventoryStatusViewModel)
                {
                    InventoryStatusViewModel inventoryStatusViewModel = (InventoryStatusViewModel)_currentViewModel;
                    if (parameter.ToString().Contains("Norm"))
                    {
                        inventoryStatusViewModel.WindowHelper.Close();

                        inventoryStatusViewModel.QuantityCommandParameter = "Quantity";
                        AddQuantityToNormWindow addQuantityToNormWindow = new AddQuantityToNormWindow(inventoryStatusViewModel);
                        addQuantityToNormWindow.ShowDialog();

                        inventoryStatusViewModel.WindowHelper = addQuantityToNormWindow;
                    }
                    else if (parameter.ToString().Contains("Quantity"))
                    {
                        try
                        {
                            if (inventoryStatusViewModel.NormQuantity > 0)
                            {
                                if (!inventoryStatusViewModel.QuantityCommandParameter.ToLower().Contains("quantityedit"))
                                {
                                    var normItem = inventoryStatusViewModel.Norma.FirstOrDefault(norm =>
                                    norm.Item.Id == inventoryStatusViewModel.CurrentInventoryStatusNorm.Item.Id);

                                    if (normItem != null)
                                    {
                                        normItem.Quantity += inventoryStatusViewModel.NormQuantity;
                                    }
                                    else
                                    {
                                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                                        var group = sqliteDbContext.ItemGroups.Find(inventoryStatusViewModel.CurrentInventoryStatusNorm.IdGroupItems);

                                        if (group != null)
                                        {
                                            bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;

                                            inventoryStatusViewModel.Norma.Add(new Invertory(inventoryStatusViewModel.CurrentInventoryStatusNorm.Item,
                                            inventoryStatusViewModel.CurrentInventoryStatusNorm.IdGroupItems,
                                            inventoryStatusViewModel.NormQuantity,
                                            0,
                                            0,
                                            isSirovina));
                                        }
                                        else
                                        {
                                            MessageBox.Show("Artikal ne pripada ni jednoj grupi!!!",
                                                "Greška",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Error);
                                        }
                                    }
                                    inventoryStatusViewModel.NormQuantityString = "0";
                                }
                                inventoryStatusViewModel.WindowHelper.Close();
                            }
                            else
                            {
                                MessageBox.Show("KOLIČINA MORA BITI BROJ!", 
                                    "Greška", 
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("GREŠKA U KREIRANJU NORMATIVA (KOLIČINA MORA BITI BROJ)!",
                                "Greška", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                        }
                    }
                }
                else if (_currentViewModel is CalculationViewModel)
                {
                    CalculationViewModel calculationViewModel = (CalculationViewModel)_currentViewModel;
                    if (parameter.ToString().Contains("Calculation"))
                    {
                        calculationViewModel.Window.Close();

                        calculationViewModel.QuantityCommandParameter = "Quantity";
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        var group = sqliteDbContext.ItemGroups.Find(calculationViewModel.CurrentInventoryStatusCalculation.IdGroupItems);

                        if(group == null)
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
                            calculationViewModel.VisibilityProsecnaPrice = Visibility.Visible;

                            calculationViewModel.ProsecnaPrice = calculationViewModel.CurrentInventoryStatusCalculation.Item.InputUnitPrice != null &&
                                calculationViewModel.CurrentInventoryStatusCalculation.Item.InputUnitPrice.HasValue ? calculationViewModel.CurrentInventoryStatusCalculation.Item.InputUnitPrice.Value : 0;
                        }
                        else
                        {
                            calculationViewModel.VisibilityProsecnaPrice = Visibility.Hidden;

                            calculationViewModel.OldPrice = calculationViewModel.CurrentInventoryStatusCalculation.Item.SellingUnitPrice;
                            calculationViewModel.NewPrice = calculationViewModel.CurrentInventoryStatusCalculation.Item.SellingUnitPrice;
                        }

                        AddQuantityToCalculationWindow addQuantityToCalculationWindow = new AddQuantityToCalculationWindow(calculationViewModel);
                        calculationViewModel.Window = addQuantityToCalculationWindow;
                        addQuantityToCalculationWindow.ShowDialog();
                    }
                    else if (parameter.ToString().Contains("Quantity"))
                    {
                        try
                        {
                            if (!calculationViewModel.QuantityCommandParameter.ToLower().Contains("quantityedit"))
                            {
                                var calculationItem = calculationViewModel.Calculations.FirstOrDefault(cal => 
                                cal.Item.Id == calculationViewModel.CurrentInventoryStatusCalculation.Item.Id);

                                if (calculationItem != null)
                                {
                                    calculationItem.Quantity += calculationViewModel.CalculationQuantity;

                                    if(calculationViewModel.CalculationPrice == 0)
                                    {
                                        decimal unitPrice = calculationViewModel.CalculationUnitPrice;

                                        if(calculationViewModel.CalculationRabat > 0)
                                        {
                                            unitPrice = Decimal.Round(calculationViewModel.CalculationUnitPrice * (1 - calculationViewModel.CalculationRabat / 100), 2);
                                        }

                                        unitPrice = unitPrice * (1 + calculationViewModel.CalculationPDV / 100);

                                        calculationViewModel.CalculationPrice = Decimal.Round(unitPrice * calculationViewModel.CalculationQuantity, 2);
                                    }

                                    calculationItem.InputPrice += calculationViewModel.CalculationPrice;
                                    calculationViewModel.TotalCalculation += calculationViewModel.CalculationPrice;
                                }
                                else
                                {
                                    SqliteDbContext sqliteDbContext = new SqliteDbContext();
                                    var group = sqliteDbContext.ItemGroups.Find(calculationViewModel.CurrentInventoryStatusCalculation.IdGroupItems);

                                    if (group == null)
                                    {
                                        MessageBox.Show("ARTIKAL MORA DA PRIPADA NEKOJ GRUPI!",
                                            "Greška",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);

                                        return;
                                    }

                                    bool isSirovina = group.Name.ToLower().Contains("sirovina") || group.Name.ToLower().Contains("sirovine") ? true : false;
                                    if (isSirovina)
                                    {

                                    }
                                    else
                                    {
                                        if (calculationViewModel.OldPrice != calculationViewModel.NewPrice)
                                        {
                                            calculationViewModel.CurrentInventoryStatusCalculation.Item.SellingUnitPrice = calculationViewModel.NewPrice;
                                        }
                                    }

                                    if (calculationViewModel.CalculationPrice == 0)
                                    {
                                        decimal unitPrice = calculationViewModel.CalculationUnitPrice;

                                        if (calculationViewModel.CalculationRabat > 0)
                                        {
                                            unitPrice = Decimal.Round(calculationViewModel.CalculationUnitPrice * (1 - calculationViewModel.CalculationRabat / 100), 2);
                                        }

                                        unitPrice = unitPrice * (1 + calculationViewModel.CalculationPDV / 100);

                                        calculationViewModel.CalculationPrice = Decimal.Round(unitPrice * calculationViewModel.CalculationQuantity, 2);
                                    }

                                    Invertory invertory = new Invertory(
                                    calculationViewModel.CurrentInventoryStatusCalculation.Item,
                                    calculationViewModel.CurrentInventoryStatusCalculation.IdGroupItems,
                                    calculationViewModel.CalculationQuantity,
                                    calculationViewModel.CalculationPrice,
                                    0,
                                    isSirovina);

                                    calculationViewModel.Calculations.Add(invertory);
                                    calculationViewModel.TotalCalculation += calculationViewModel.CalculationPrice;
                                }

                                calculationViewModel.CalculationQuantityString = "0";
                                calculationViewModel.CalculationPriceString = "0";
                                calculationViewModel.CalculationUnitPriceString = "0";
                                calculationViewModel.CalculationRabatString = "0";
                                calculationViewModel.OldPrice = 0;
                                calculationViewModel.NewPriceString = "0";
                                calculationViewModel.ProsecnaPrice = 0;
                            }
                            calculationViewModel.Window.Close();
                        }
                        catch
                        {
                            MessageBox.Show("Greška u kreiranju kalkulacije!",
                                "Greška",
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}