using UniversalEsir.Models.Sale;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Otpremnice;
using UniversalEsir_Database;
using UniversalEsir_Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Otpremnice
{
    public class OpenItemsInOtpremnicaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OtpremniceViewModel _currentViewModel;

        public OpenItemsInOtpremnicaCommand(OtpremniceViewModel currentViewModel)
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
                _currentViewModel.AllItemsInOtpremnica = new ObservableCollection<ItemInvoice>();
                if (parameter != null &&
                    parameter is string)
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    _currentViewModel.CurrentOtpremnice = _currentViewModel.Otpremnice.FirstOrDefault(otp => otp.Id == parameter.ToString());

                    if(_currentViewModel.CurrentOtpremnice != null )
                    {
                        var itemsInOtpremnica = sqliteDbContext.ItemInvoices.Where(item => 
                        item.InvoiceId == _currentViewModel.CurrentOtpremnice.Id);

                        if(itemsInOtpremnica != null &&
                            itemsInOtpremnica.Any())
                        {
                            itemsInOtpremnica.ForEachAsync(itemInInvoice =>
                            {
                                var itemDB = sqliteDbContext.Items.Find(itemInInvoice.ItemCode);

                                if (itemDB != null)
                                {
                                    Item item = new Item(itemDB);
                                    ItemInvoice itemInvoice = new ItemInvoice(item, itemInInvoice);

                                    _currentViewModel.AllItemsInOtpremnica.Add(itemInvoice);
                                }
                            });

                            _currentViewModel.Window = new ItemInOtpremnicaWindow(_currentViewModel);
                            _currentViewModel.Window.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"OpenItemsInOtpremnicaCommand -> Execute -> Desila se greska: ", ex);
                MessageBox.Show("Neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}