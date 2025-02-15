using UniversalEsir.Models.Sale;
using UniversalEsir.Models.TableOverview;
using UniversalEsir.ViewModels;
using UniversalEsir_Common.Enums;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Logging;
using UniversalEsir_Printer.PaperFormat;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace UniversalEsir.Commands.TableOverview
{
    public class ClickOnPaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TableOverviewViewModel _viewModel;

        public ClickOnPaymentPlaceCommand(TableOverviewViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (parameter is Order)
            {
                Order order = (Order)parameter;

                if (_viewModel.SaleViewModel.CurrentOrder is null)
                {
                    Log.Debug("ClickOnPaymentPlaceCommand - Execute - Naplati porudzbinu");

                    ChargeOrder(order);
                }
                else
                {
                    Log.Debug("ClickOnPaymentPlaceCommand - Execute - Zakaci porudzbinu");

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();
                    var unprocessedOrder = sqliteDbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == order.TableId);

                    if (unprocessedOrder != null)
                    {
                        try
                        {
                            Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Dodavanje na porudzbinu {unprocessedOrder.Id} koja je na stolu {unprocessedOrder.PaymentPlaceId}");
                            var itemsInUnprocessedOrder = sqliteDbContext.ItemsInUnprocessedOrder.Where(item => item.UnprocessedOrderId == unprocessedOrder.Id);

                            if (itemsInUnprocessedOrder != null && itemsInUnprocessedOrder.Any())
                            {
                                itemsInUnprocessedOrder.ToList().ForEach(item =>
                                {
                                    sqliteDbContext.ItemsInUnprocessedOrder.Remove(item);
                                });
                            }

                            sqliteDbContext.UnprocessedOrders.Remove(unprocessedOrder);
                            sqliteDbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"ClickOnPaymentPlaceCommand - Execute - GRESKA prilikom dodavanja na porudzbinu {unprocessedOrder.Id} koja je na stolu {unprocessedOrder.PaymentPlaceId}");
                            return;
                        }
                    }

                    PaymentPlace? paymentPlace = _viewModel.NormalPaymentPlaces.FirstOrDefault(pp => pp.Order.TableId == order.TableId);

                    if(paymentPlace == null)
                    {
                        paymentPlace = _viewModel.RoundPaymentPlaces.FirstOrDefault(pp => pp.Order.TableId == order.TableId);
                    }

                    if (paymentPlace != null)
                    {
                        if (order.Items == null || !order.Items.Any())
                        {
                            Log.Debug("ClickOnPaymentPlaceCommand - Execute - Kreiranje nove porudzbine!");
                            AddNewOrder(order, paymentPlace);

                            Task.Run(() =>
                            {
                                PrintOrder(paymentPlace.Order.Cashier.Name, order.TableId, order.Items.ToList());
                            });
                        }
                        else
                        {
                            Log.Debug("ClickOnPaymentPlaceCommand - Execute - Dodavanje nove porudzbine na vec postojecu!");
                            List<ItemInvoice> items = new List<ItemInvoice>(_viewModel.SaleViewModel.CurrentOrder.Items);
                            Task.Run(() =>
                            {
                                PrintOrder(paymentPlace.Order.Cashier.Name, order.TableId, items);
                            });

                            if (paymentPlace.Order.Items.Any())
                            {
                                paymentPlace.Order.Items.ToList().ForEach(item =>
                                {
                                    if (_viewModel.SaleViewModel.CurrentOrder.Items != null &&
                                    _viewModel.SaleViewModel.CurrentOrder.Items.Any())
                                    {
                                        var i = _viewModel.SaleViewModel.CurrentOrder.Items.FirstOrDefault(it => it.Item.Id == item.Item.Id);
                                        if (i != null)
                                        {
                                            i.TotalAmout += item.TotalAmout;
                                            i.Quantity += item.Quantity;
                                        }
                                        else
                                        {
                                            _viewModel.SaleViewModel.CurrentOrder.Items.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        if (_viewModel.SaleViewModel.CurrentOrder.Items == null)
                                        {
                                            _viewModel.SaleViewModel.CurrentOrder.Items = new ObservableCollection<ItemInvoice>();
                                        }

                                        _viewModel.SaleViewModel.CurrentOrder.Items.Add(item);
                                    }
                                });
                            }
                            paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                            paymentPlace.Total = 0;
                            AddNewOrder(order, paymentPlace);
                        }
                    }
                }
                Log.Debug("ClickOnPaymentPlaceCommand - Execute - Uspesno kreirana porudzbina!");
            }
        }
        private void ChargeOrder(Order order)
        {
            PaymentPlace? paymentPlace = _viewModel.NormalPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);

            if(paymentPlace == null)
            {
                paymentPlace = _viewModel.RoundPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);
            }

            if (paymentPlace != null)
            {
                if (paymentPlace.Order.Items != null &&
                    paymentPlace.Order.Items.Any())
                {
                    _viewModel.SaleViewModel.TableId = order.TableId;
                    _viewModel.SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items);
                    _viewModel.SaleViewModel.TotalAmount = paymentPlace.Total;
                    _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId)
                    {
                        Items = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items),
                        Cashier = order.Cashier
                    };
                    paymentPlace.Background = Brushes.Green;
                    paymentPlace.Order = null;
                    paymentPlace.Total = 0;
                    _viewModel.CancelCommand.Execute(null);
                }
            }
        }
        private void AddNewOrder(Order order, PaymentPlace paymentPlace)
        {
            if (_viewModel.SaleViewModel.CurrentOrder.Items is not null)
            {
                if (paymentPlace.Background == Brushes.Green)
                {
                    paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                    paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                    paymentPlace.Background = Brushes.Red;
                }

                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                UnprocessedOrderDB unprocessedOrderDB = new UnprocessedOrderDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    CashierId = paymentPlace.Order.Cashier.Id,
                    PaymentPlaceId = paymentPlace.Id,
                    TotalAmount = paymentPlace.Total
                };

                sqliteDbContext.UnprocessedOrders.Add(unprocessedOrderDB);
                sqliteDbContext.SaveChanges();

                _viewModel.SaleViewModel.CurrentOrder.Items.ToList().ForEach(item =>
                {
                    ItemInUnprocessedOrderDB itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                    {
                        ItemId = item.Item.Id,
                        UnprocessedOrderId = unprocessedOrderDB.Id,
                        Quantity = item.Quantity
                    };
                    sqliteDbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);

                    if (paymentPlace.Order.Items != null &&
                        paymentPlace.Order.Items.Any())
                    {
                        var i = paymentPlace.Order.Items.Where(it => it.Item.Id == item.Item.Id).ToList().FirstOrDefault();

                        if (i != null)
                        {
                            i.TotalAmout += item.TotalAmout;
                            i.Quantity += item.Quantity;
                        }
                        else
                        {
                            paymentPlace.Order.Items.Add(item);
                        }
                    }
                    else
                    {
                        if (paymentPlace.Order.Items == null)
                        {
                            paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                        }

                        paymentPlace.Order.Items.Add(item);
                    }

                    paymentPlace.Total += item.TotalAmout;
                    sqliteDbContext.SaveChanges();
                });

                unprocessedOrderDB.TotalAmount = paymentPlace.Total;
                sqliteDbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                sqliteDbContext.SaveChanges();

#if CRNO
                _viewModel.SaleViewModel.LogoutCommand.Execute(true);
#else
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    _viewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _viewModel.SaleViewModel.Reset();
                }
#endif

            }
        }

        private void PrintOrder(string loggedCashier, int tableId, List<ItemInvoice> items)
        {
            var sqliteDbContext = new SqliteDbContext();

            DateTime orderTime = DateTime.Now;

            UniversalEsir_Common.Models.Order.Order orderKuhinja = new UniversalEsir_Common.Models.Order.Order()
            {
                CashierName = loggedCashier,
                TableId = tableId,
                Items = new List<UniversalEsir_Common.Models.Order.ItemOrder>(),
                OrderTime = orderTime
            };
            UniversalEsir_Common.Models.Order.Order orderSank = new UniversalEsir_Common.Models.Order.Order()
            {
                CashierName = loggedCashier,
                TableId = tableId,
                Items = new List<UniversalEsir_Common.Models.Order.ItemOrder>(),
                OrderTime = orderTime
            };
            var partHall = sqliteDbContext.PartHalls.Join(sqliteDbContext.PaymentPlaces,
                partHall => partHall.Id,
                table => table.PartHallId,
                (partHall, table) => new { PartHall = partHall, Table = table })
                .FirstOrDefault(t => t.Table.Id == tableId);

            if (partHall != null)
            {
                orderKuhinja.PartHall = partHall.PartHall.Name;
                orderSank.PartHall = partHall.PartHall.Name;
            }

            items.ForEach(item =>
            {
                var itemNadgroup = sqliteDbContext.Items.Join(sqliteDbContext.ItemGroups,
                item => item.IdItemGroup,
                itemGroup => itemGroup.Id,
                (item, itemGroup) => new { Item = item, ItemGroup = itemGroup })
                .Join(sqliteDbContext.Supergroups,
                group => group.ItemGroup.IdSupergroup,
                supergroup => supergroup.Id,
                (group, supergroup) => new { Group = group, Supergroup = supergroup })
                .FirstOrDefault(it => it.Group.Item.Id == item.Item.Id);

                if (itemNadgroup != null)
                {
                    if (itemNadgroup.Supergroup.Name.ToLower().Contains("hrana") ||
                    itemNadgroup.Supergroup.Name.ToLower().Contains("kuhinja"))
                    {
                        orderKuhinja.Items.Add(new UniversalEsir_Common.Models.Order.ItemOrder()
                        {
                            Name = item.Item.Name,
                            Quantity = item.Quantity,
                        });
                    }
                    else
                    {
                        orderSank.Items.Add(new UniversalEsir_Common.Models.Order.ItemOrder()
                        {
                            Name = item.Item.Name,
                            Quantity = item.Quantity
                        });
                    }
                }
                else
                {
                    orderSank.Items.Add(new UniversalEsir_Common.Models.Order.ItemOrder()
                    {
                        Name = item.Item.Name,
                        Quantity = item.Quantity
                    });
                }
            });


            var posType = SettingsManager.Instance.GetPrinterFormat() == PrinterFormatEnumeration.Pos80mm ?
            UniversalEsir_Printer.Enums.PosTypeEnumeration.Pos80mm : UniversalEsir_Printer.Enums.PosTypeEnumeration.Pos58mm;
            if (orderSank.Items.Any())
            {
                FormatPos.PrintOrder(orderSank, posType, UniversalEsir_Printer.Enums.OrderTypeEnumeration.Sank);
            }
            if (orderKuhinja.Items.Any())
            {
                FormatPos.PrintOrder(orderKuhinja, posType, UniversalEsir_Printer.Enums.OrderTypeEnumeration.Kuhinja);
            }
        }
    }
}
