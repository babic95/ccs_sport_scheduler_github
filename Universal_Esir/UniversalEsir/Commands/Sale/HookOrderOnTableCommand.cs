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

            if (_viewModel.TableId > 0)
            {
                SqliteDbContext sqliteDbContext = new SqliteDbContext();

                var unprocessedOrder = sqliteDbContext.UnprocessedOrders.FirstOrDefault(table => table.PaymentPlaceId == _viewModel.TableId);

                if(unprocessedOrder != null)
                {
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
            }

            _viewModel.CurrentOrder = new Order(_viewModel.LoggedCashier, _viewModel.ItemsInvoice);

            AppStateParameter appStateParameter = new AppStateParameter(AppStateEnumerable.TableOverview,
                _viewModel.LoggedCashier,
                _viewModel);
            _viewModel.UpdateAppViewModelCommand.Execute(appStateParameter);
        }
    }
}
