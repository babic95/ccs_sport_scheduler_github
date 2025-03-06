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
using System.Windows;
using SQLitePCL;
using Microsoft.EntityFrameworkCore;

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

            try
            {
                if (parameter is Order)
                {
                    Order order = (Order)parameter;

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();
                    if (_viewModel.SaleViewModel.CurrentOrder is null ||
                        _viewModel.SaleViewModel.CurrentOrder.TableId != order.TableId ||
                        _viewModel.SaleViewModel.TableId == 0 ||
                        _viewModel.SaleViewModel.ItemsInvoice == null ||
                        !_viewModel.SaleViewModel.ItemsInvoice.Any())
                    {
                        Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Naplati / pregledaj porudzbinu sa stola {order.TableId}");

                        ChargeOrder(order, sqliteDbContext);
                    }
                    else
                    {
                        Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Zakaci porudzbinu na sto {order.TableId}");


                        var unprocessedOrderDB = sqliteDbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == order.TableId);

                        PaymentPlace? paymentPlace = _viewModel.NormalPaymentPlaces.FirstOrDefault(pp => pp.Order.TableId == order.TableId);

                        if (paymentPlace == null)
                        {
                            paymentPlace = _viewModel.RoundPaymentPlaces.FirstOrDefault(pp => pp.Order.TableId == order.TableId);
                        }

                        if (paymentPlace != null)
                        {
                            if (unprocessedOrderDB != null)
                            {
                                Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Kreiranje nove porudzbine na vec postojecu porudzbinu na stolu {order.TableId}!");
                                AddToOldOrder(order, paymentPlace, unprocessedOrderDB, sqliteDbContext);
                            }
                            else
                            {
                                unprocessedOrderDB = new UnprocessedOrderDB()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CashierId = _viewModel.SaleViewModel.CurrentOrder.Cashier.Id,
                                    PaymentPlaceId = paymentPlace.Id,
                                    TotalAmount = 0//paymentPlace.Total
                                };
                                Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Kreiranje nove porudzbine na stolu {order.TableId}!");
                                AddNewOrder(order, paymentPlace, unprocessedOrderDB, sqliteDbContext);
                            }
                        }
                    }
                    Log.Debug($"ClickOnPaymentPlaceCommand - Execute - Uspesno kreirana porudzbina na stolu {order.TableId}!");
                }

            }
            catch (Exception ex)
            {
                Log.Error($"ClickOnPaymentPlaceCommand - Execute - Greska na stolu {parameter.ToString()}: ", ex);
                MessageBox.Show("Desila se greška, pokušajte ponovo.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private async void ChargeOrder(Order order, SqliteDbContext sqliteDbContext)
        {
            try
            {
                PaymentPlace? paymentPlace = _viewModel.NormalPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);

                if (paymentPlace == null)
                {
                    paymentPlace = _viewModel.RoundPaymentPlaces.FirstOrDefault(pp => pp.Id == order.TableId);
                }

                if (paymentPlace != null)
                {
                    _viewModel.SaleViewModel.TableId = order.TableId;
                    _viewModel.SaleViewModel.CurrentClan = new Models.AppMain.Statistic.Clanovi.Clan()
                    {
                        Username = paymentPlace.Name,
                        FullName = paymentPlace.Name,
                        Id = paymentPlace.UserId,
                    };

                    if (paymentPlace.Order.Items != null &&
                        paymentPlace.Order.Items.Any())
                    {
                        _viewModel.SaleViewModel.OldOrders = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items);
                        _viewModel.SaleViewModel.TotalAmount = paymentPlace.Total;
                        
                        _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId, paymentPlace.Name)
                        {
                            Items = new ObservableCollection<ItemInvoice>(paymentPlace.Order.Items),
                            Cashier = order.Cashier
                        };
                        //paymentPlace.Background = Brushes.Green;
                        //paymentPlace.Order = null;
                        //paymentPlace.Total = 0;
                    }
                    else
                    {
                        _viewModel.SaleViewModel.OldOrders = new ObservableCollection<ItemInvoice>();
                        _viewModel.SaleViewModel.ItemsInvoice = new ObservableCollection<ItemInvoice>();
                        _viewModel.SaleViewModel.TotalAmount = 0;

                        _viewModel.SaleViewModel.CurrentOrder = new Order(paymentPlace.Id, paymentPlace.PartHallId, paymentPlace.Name)
                        {
                            Items = new ObservableCollection<ItemInvoice>(),
                            Cashier = _viewModel.SaleViewModel.LoggedCashier
                        };
                    }
                    _viewModel.CancelCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Log.Error("ClickOnPaymentPlaceCommand -> ChargeOrder -> Greska prilikom naplate: ", ex);
            }
        }
        private void AddToOldOrder(Order order,
            PaymentPlace paymentPlace,
            UnprocessedOrderDB unprocessedOrderDB, 
            SqliteDbContext sqliteDbContext)
        {
            if (_viewModel.SaleViewModel.CurrentOrder.Items is not null)
            {
                if (paymentPlace.Background == Brushes.Green)
                {
                    paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                    paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                    paymentPlace.Background = Brushes.Red;
                }

                _viewModel.SaleViewModel.ItemsInvoice.ToList().ForEach(item =>
                {
                    var itemInUnprocessedOrderDB = sqliteDbContext.ItemsInUnprocessedOrder.FirstOrDefault(i => i.ItemId == item.Item.Id &&
                    i.UnprocessedOrderId == unprocessedOrderDB.Id);

                    if (itemInUnprocessedOrderDB == null)
                    {
                        itemInUnprocessedOrderDB = new ItemInUnprocessedOrderDB()
                        {
                            ItemId = item.Item.Id,
                            UnprocessedOrderId = unprocessedOrderDB.Id,
                            Quantity = item.Quantity
                        };
                        sqliteDbContext.ItemsInUnprocessedOrder.Add(itemInUnprocessedOrderDB);
                    }
                    else
                    {
                        itemInUnprocessedOrderDB.Quantity += item.Quantity;
                        sqliteDbContext.ItemsInUnprocessedOrder.Update(itemInUnprocessedOrderDB);
                    }
                    unprocessedOrderDB.TotalAmount += item.TotalAmout;
                    unprocessedOrderDB.CashierId = _viewModel.SaleViewModel.LoggedCashier.Id;
                    sqliteDbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                    //sqliteDbContext.SaveChanges();

                    if (paymentPlace.Order.Items != null &&
                        paymentPlace.Order.Items.Any())
                    {
                        var i = paymentPlace.Order.Items.FirstOrDefault(it => it.Item.Id == item.Item.Id);

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
                    //sqliteDbContext.SaveChanges();
                });

                sqliteDbContext.SaveChanges();

                if (SettingsManager.Instance.EnableSmartCard())
                {
                    _viewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _viewModel.SaleViewModel.Reset();
                }
            }

        }
        private void AddNewOrder(Order order,
            PaymentPlace paymentPlace,
            UnprocessedOrderDB unprocessedOrderDB,
            SqliteDbContext sqliteDbContext)
        {
            if (_viewModel.SaleViewModel.CurrentOrder.Items is not null)
            {
                if (paymentPlace.Background == Brushes.Green)
                {
                    paymentPlace.Order.Items = new ObservableCollection<ItemInvoice>();
                    paymentPlace.Order.Cashier = _viewModel.SaleViewModel.LoggedCashier;
                    paymentPlace.Background = Brushes.Red;
                }

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
                    //sqliteDbContext.SaveChanges();
                });

                unprocessedOrderDB.TotalAmount = paymentPlace.Total;
                sqliteDbContext.UnprocessedOrders.Update(unprocessedOrderDB);
                sqliteDbContext.SaveChanges();

                if (SettingsManager.Instance.EnableSmartCard())
                {
                    _viewModel.SaleViewModel.LogoutCommand.Execute(true);
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _viewModel.SaleViewModel.Reset();
                }
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
