using UniversalEsir.Enums;
using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels;
using UniversalEsir_Common.Enums;
using UniversalEsir_Common.Models.Invoice;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Printer.PaperFormat;
using UniversalEsir_Settings;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.Sale
{
    public class HookOrderOnTableCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private SaleViewModel _viewModel;

        public HookOrderOnTableCommand(SaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            _viewModel.TableOverviewViewModel = new TableOverviewViewModel(_viewModel);

            if (_viewModel.TableId == 0)
            {
                _viewModel.TableOverviewViewModel = new TableOverviewViewModel(_viewModel);

                _viewModel.CurrentOrder = new Order(_viewModel.LoggedCashier, _viewModel.ItemsInvoice)
                {
                    TableId = _viewModel.TableId,
                };

                AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                    _viewModel.LoggedCashier,
                    _viewModel);
                _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
            }
            else
            {
                try
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();
                    var tableDB = sqliteDbContext.PaymentPlaces.Find(_viewModel.TableId);

                    var unprocessedOrderDB = sqliteDbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == _viewModel.TableId);

                    if (unprocessedOrderDB != null)
                    {
                        foreach (var item in _viewModel.ItemsInvoice)
                        {
                            var itemInUnprocessedOrderDB = sqliteDbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.UnprocessedOrderId == unprocessedOrderDB.Id &&
                            i.ItemId == item.Item.Id);

                            if (itemInUnprocessedOrderDB == null)
                            {
                                itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                                {
                                    ItemId = item.Item.Id,
                                    Quantity = item.Quantity,
                                    UnprocessedOrderId = unprocessedOrderDB.Id,
                                };

                                sqliteDbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);
                            }
                            else
                            {
                                itemInUnprocessedOrderDB.Quantity += item.Quantity;
                                //_viewModel.DbContext.ItemsInUnprocessedOrder.Update(itemInUnprocessedOrderDB);
                            }

                            unprocessedOrderDB.TotalAmount += item.TotalAmout;
                            unprocessedOrderDB.CashierId = _viewModel.LoggedCashier.Id;
                            sqliteDbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                        }
                    }
                    else
                    {
                        unprocessedOrderDB = new UnprocessedOrderDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CashierId = _viewModel.LoggedCashier.Id,
                            PaymentPlaceId = _viewModel.CurrentOrder.TableId,
                            TotalAmount = 0,
                            ItemsInUnprocessedOrder = new List<ItemInUnprocessedOrderDB>(),
                        };

                        _viewModel.ItemsInvoice.ToList().ForEach(item =>
                        {
                            var itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                            {
                                ItemId = item.Item.Id,
                                Quantity = item.Quantity,
                                UnprocessedOrderId = unprocessedOrderDB.Id,
                            };

                            unprocessedOrderDB.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);

                            unprocessedOrderDB.TotalAmount += item.TotalAmout;
                        });
                        sqliteDbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                    }

                    bool saveFailed;
                    int retryCount = 0;
                    do
                    {
                        saveFailed = false;
                        try
                        {
                            sqliteDbContext.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            saveFailed = true;
                            retryCount++;

                            // Update the values of the entity that failed to save from the store
                            ex.Entries.Single().Reload();

                            if (retryCount >= 3)
                            {
                                throw;
                            }
                        }
                    } while (saveFailed);

                    var loggedCashier = _viewModel.LoggedCashier;
                    var tableId = _viewModel.TableId;
                    var itemsInvoice = _viewModel.ItemsInvoice.ToList();
                    var unprocessedOrder = unprocessedOrderDB;


                    _viewModel.Reset();

                    AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                        _viewModel.LoggedCashier,
                        _viewModel);
                    _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
                }
                catch (Exception ex)
                {
                    Log.Error("HookOrderOnTableCommand -> Execute -> Greska prilikom kreiranja porudzbine na vec postojecu: ", ex);
                    MessageBox.Show("Desila se greška prilikom kreiranja porudžbine!\nObratite se serviseru.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
