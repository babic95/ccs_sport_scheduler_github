using UniversalEsir.Models.AppMain.Statistic.Driver;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using UniversalEsir_Logging;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class OpenItemsInIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public OpenItemsInIsporukaCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            _currentViewModel.CurrentInvoice = null;
            _currentViewModel.ItemsInInvoice = new ObservableCollection<Models.Sale.ItemInvoice>();
            try
            {
                if (parameter != null)
                {
                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var invoceDB = await sqliteDbContext.Invoices.FindAsync(parameter.ToString());

                    if(invoceDB == null)
                    {
                        Log.Error($"OpenItemsInIsporukaCommand -> Execute -> Racun sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Račun ne postoji u bazi podataka.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var itemInInvoice = sqliteDbContext.ItemInvoices.Where(item => item.InvoiceId == parameter.ToString());

                    if (itemInInvoice == null ||
                        !itemInInvoice.Any())
                    {
                        Log.Error($"OpenItemsInIsporukaCommand -> Execute -> Nema artikala u racunu sa ID = {parameter} ne postoji u bazi!");
                        MessageBox.Show("Nema artikala za izabran račun u bazi podataka.\nObratite se serviseru.",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    _currentViewModel.CurrentInvoice = new Models.Sale.Invoice(invoceDB, 0);

                    await itemInInvoice.ForEachAsync(itemInInvoice =>
                    {
                        var itemDB = sqliteDbContext.Items.Find(itemInInvoice.ItemCode);

                        if (itemDB != null) 
                        {
                            Models.Sale.Item item = new Models.Sale.Item(itemDB);
                            Models.Sale.ItemInvoice itemInvoice = new Models.Sale.ItemInvoice(item, itemInInvoice);

                            _currentViewModel.ItemsInInvoice.Add(itemInvoice);
                        }
                    });

                    _currentViewModel.WindowItemsInInvoice = new ItemsInInvoiceWindow(_currentViewModel);
                    _currentViewModel.WindowItemsInInvoice.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"OpenIsporukaCommand -> Execute -> Neocekivana greska: ", ex);
                MessageBox.Show("Desila se neočekivana greška.\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}