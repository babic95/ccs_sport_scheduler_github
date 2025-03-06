using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Common.Models.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Printer;

namespace UniversalEsir.Commands.AppMain.Statistic.ViewCalculation
{
    public class PrintCalculationA4Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ViewCalculationViewModel _currentViewModel;

        public PrintCalculationA4Command(ViewCalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (parameter is string)
            {
                string calculationId = (string)parameter;
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var calculation = _currentViewModel.Calculations.FirstOrDefault(x => x.Id == calculationId);
                var calculationDB = sqliteDbContext.Calculations.FirstOrDefault(x => x.Id == calculationId);

                if (calculationDB != null)
                {
                    List<InvertoryGlobal> calculationItemsGlobal = new List<InvertoryGlobal>();

                    var calculationItems = await _currentViewModel.GetAllItemsInCalculation(calculationDB);
                    if (calculationItems != null &&
                        calculationItems.Any())
                    {
                        calculationItems.ToList().ForEach(item =>
                        {
                            var itemDB = sqliteDbContext.Items.Find(item.Item.Id);

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

                                decimal taxP = 0;

                                switch (itemDB.Label)
                                {
                                    case "1":
                                        taxP = 0;
                                        break;
                                    case "6":
                                        taxP = 20;
                                        break;
                                    case "7":
                                        taxP = 10;
                                        break;
                                    case "4":
                                        taxP = 0;
                                        break;
                                    case "31":
                                        taxP = 9;
                                        break;
                                    case "47":
                                        taxP = 0;
                                        break;
                                    case "8":
                                        taxP = 19;
                                        break;
                                    case "39":
                                        taxP = 11;
                                        break;
                                }

                                decimal inputUnitPrice = Decimal.Round(item.InputPrice / item.Quantity, 2);
                                if (group.Name.ToLower().Contains("sirovine") ||
                                    group.Name.ToLower().Contains("sirovina"))
                                {
                                    calculationItemsGlobal.Add(new InvertoryGlobal()
                                    {
                                        Id = item.Item.Id,
                                        Name = item.Item.Name,
                                        Jm = item.Item.Jm,
                                        InputUnitPrice = inputUnitPrice,
                                        SellingUnitPrice = 0,
                                        Quantity = item.Quantity,
                                        TotalAmout = Decimal.Round(item.InputPrice, 2),
                                        Tax = taxP,
                                    });
                                }
                                else
                                {
                                    calculationItemsGlobal.Add(new InvertoryGlobal()
                                    {
                                        Id = item.Item.Id,
                                        Name = item.Item.Name,
                                        Jm = item.Item.Jm,
                                        InputUnitPrice = inputUnitPrice,
                                        SellingUnitPrice = item.Item.SellingUnitPrice,
                                        Quantity = item.Quantity,
                                        TotalAmout = Decimal.Round(item.TotalAmout * item.Quantity, 2),
                                        Tax = taxP,
                                    });
                                }
                            }
                        });
                    }

                    var suppDB = sqliteDbContext.Suppliers.Find(calculationDB.SupplierId);

                    if (suppDB != null)
                    {
                        PrinterManager.Instance.PrintCalculationA4Status(calculationDB, calculationItemsGlobal, suppDB);
                    }
                }
            }
        }
    }
}