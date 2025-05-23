﻿using UniversalEsir.ViewModels;
using UniversalEsir_Database;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace UniversalEsir.Commands.Sale
{
    public class RemoveOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public RemoveOrderCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da obrišete trenutnu porudžbinu?", "Brisanje porudžbine",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_viewModel.TableId > 0 &&
                        SettingsManager.Instance.CancelOrderFromTable())
                    {
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        var order = sqliteDbContext.UnprocessedOrders.FirstOrDefault(order => order.PaymentPlaceId == _viewModel.TableId);

                        if (order != null)
                        {
                            var itemsInOrder = sqliteDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == order.Id);

                            if (itemsInOrder != null &&
                                itemsInOrder.Any())
                            {
                                itemsInOrder.ToList().ForEach(item =>
                                {
                                    sqliteDbContext.ItemsInUnprocessedOrder.Remove(item);
                                });
                            }
                            sqliteDbContext.UnprocessedOrders.Remove(order);
                            sqliteDbContext.SaveChanges();
                        }
                    }

                    _viewModel.Reset();
                }
                catch
                {
                    MessageBox.Show("Greška prilikom brisanja porudžbine!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
