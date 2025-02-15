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
using UniversalEsir_Logging;
using UniversalEsir_Database.Models;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver;

namespace UniversalEsir.Commands.AppMain.Statistic.Driver
{
    public class CreateIsporukaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DriverViewModel _currentViewModel;

        public CreateIsporukaCommand(DriverViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                if(_currentViewModel.CurrentIsporuka == null)
                {
                    Log.Error($"CreateIsporukaCommand -> Execute -> Greska prilikom kreiranja isporuke, CurrentIsporuka je NULL!");
                    MessageBox.Show("Greška prilikom kreiranja isporuke.\nObratite se serviseru.",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if(_currentViewModel.CurrentIsporuka.DriverInvoices == null ||
                    !_currentViewModel.CurrentIsporuka.DriverInvoices.Any())
                {
                    Log.Error($"CreateIsporukaCommand -> Execute -> Greska prilikom kreiranja isporuke, nema racuna za isporuku!");
                    MessageBox.Show("Isporuka nema porudzmenice!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var result = MessageBox.Show("Da li zaista želite da kreirate isporuku?",
                        "Kreiranje isporuke",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _currentViewModel.CurrentIsporuka.DateIsporuka = DateTime.Now;
                    _currentViewModel.Window = new SelectedDateForIsporukaWindow(_currentViewModel);
                    _currentViewModel.Window.ShowDialog();


                    if (_currentViewModel.CurrentIsporuka.DateIsporuka == null)
                    {
                        Log.Error($"CreateIsporukaCommand -> Execute -> Datum isporuke nije setovan!");
                        MessageBox.Show("Nije postavljen datum isporuke!",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        if (_currentViewModel.CurrentIsporuka.DateIsporuka.Value.Date < DateTime.Now.Date)
                        {
                            Log.Error($"CreateIsporukaCommand -> Execute -> Datum isporuke je u proslosti!");
                            MessageBox.Show("Datum isporuke ne može da bude u prošlosti!",
                                "Greška",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }
                    }

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var isporuke = sqliteDbContext.Isporuke.Where(isporuka => isporuka.CreateDate.Year == _currentViewModel.CurrentIsporuka.CreateDate.Year);

                    int counter = 1;

                    if(isporuke != null &&
                        isporuke.Any())
                    {
                        counter = isporuke.Max(i => i.Counter);
                        counter++;
                    }

                    IsporukaDB isporukaDB = new IsporukaDB()
                    {
                        Id = _currentViewModel.CurrentIsporuka.Id,
                        CreateDate = _currentViewModel.CurrentIsporuka.CreateDate,
                        DateIsporuka = _currentViewModel.CurrentIsporuka.DateIsporuka.Value,
                        Counter = counter,
                        TotalAmount = _currentViewModel.CurrentIsporuka.TotalAmount
                    };
                    sqliteDbContext.Isporuke.Add(isporukaDB);

                    _currentViewModel.CurrentIsporuka.DriverInvoices.ToList().ForEach(i =>
                    {
                        if (i.IsChecked)
                        {
                            var driverInvoice = sqliteDbContext.DriverInvoices.Find(i.Invoice.Id,
                                _currentViewModel.CurrentIsporuka.Driver.Id);

                            if (driverInvoice != null)
                            {
                                driverInvoice.IsporukaId = isporukaDB.Id;
                                sqliteDbContext.DriverInvoices.Update(driverInvoice);
                            }
                            else
                            {
                                Log.Error($"CreateIsporukaCommand -> Execute -> Greska prilikom dodavanja DriverInvoice gde je Invoice.Id = {i.Invoice.Id}!");
                                MessageBox.Show("Greška prilikom kreiranja isporuke.\nObratite se serviseru.",
                                    "Greška",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }
                        }
                    });

                    sqliteDbContext.SaveChanges();

                    _currentViewModel.Initialize();

                    MessageBox.Show("Usprešno ste kreirali porudzbinu!",
                            "Usprešno kreiranje porudzbine",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    if (_currentViewModel.WindowIsporuka != null)
                    {
                        _currentViewModel.WindowIsporuka.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CreateIsporukaCommand -> Execute -> Greska prilikom kreiranja isporuke", ex);
            }
        }
    }
}