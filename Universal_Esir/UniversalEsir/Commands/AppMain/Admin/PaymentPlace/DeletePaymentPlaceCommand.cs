﻿using UniversalEsir.Enums.AppMain.Admin;
using UniversalEsir.Models.TableOverview;
using UniversalEsir.ViewModels.AppMain;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Admin
{
    public class DeletePaymentPlaceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private AdminViewModel _currentViewModel;

        public DeletePaymentPlaceCommand(AdminViewModel currentViewModel)
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
                MessageBoxResult result = MessageBox.Show("Da li želite da obrišete platno mesto?",
                    "Brisanje platnog mesta",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int id = Convert.ToInt32(parameter);
                    PaymentPlace? paymentPlace = _currentViewModel.NormalPaymentPlaces.FirstOrDefault(p => p.Id == id);

                    if(paymentPlace == null)
                    {
                        paymentPlace = _currentViewModel.RoundPaymentPlaces.FirstOrDefault(p => p.Id == id);
                    }

                    if (paymentPlace != null)
                    {
                        SqliteDbContext sqliteDbContext = new SqliteDbContext();

                        PaymentPlaceDB? paymentPlaceDB = sqliteDbContext.PaymentPlaces.Find(id);

                        if (paymentPlaceDB != null) 
                        {
                            sqliteDbContext.PaymentPlaces.Remove(paymentPlaceDB);
                            sqliteDbContext.SaveChanges();

                            MessageBox.Show("Uspešno ste obrisali platno mesto!", "Uspešno brisanje", MessageBoxButton.OK, MessageBoxImage.Information); 

                            if(paymentPlace.Type == PaymentPlaceTypeEnumeration.Normal)
                            {
                                var payment = _currentViewModel.AllNormalPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id);

                                if (payment != null)
                                {
                                    _currentViewModel.AllNormalPaymentPlaces.Remove(payment);
                                    _currentViewModel.NormalPaymentPlaces.Remove(paymentPlace);
                                }
                            }
                            else
                            {
                                var payment = _currentViewModel.AllRoundPaymentPlaces.FirstOrDefault(p => p.Id == paymentPlace.Id);

                                if (payment != null)
                                {
                                    _currentViewModel.AllRoundPaymentPlaces.Remove(payment);
                                    _currentViewModel.RoundPaymentPlaces.Remove(paymentPlace);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom brisanja platnog mesta!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
