﻿using UniversalEsir.Commands;
using UniversalEsir.Enums;
using UniversalEsir_API;
using UniversalEsir_Common.Enums;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniversalEsir.ViewModels.Login
{
    public class LoginCardViewModel : ViewModelBase
    {
        private readonly string _fromPath = @"C:\CCS UniversalEsir\UniversalEsir_Admin\PIN.json";
        private string _message;
        private string _password;
        private ImageSource _logo;
        private Timer _timer;
        private DateTime? _validTo;
        private Task _initializationTask;
        private string _carNumber;

        private Visibility _visibilityBlack;

        public readonly CashierDB CashierAdmin2 = new CashierDB()
        {
            Id = "9876543210",
            Type = CashierTypeEnumeration.Admin,
            Name = "CleanCodeSirmium"
        };

        public LoginCardViewModel(ICommand updateCurrentViewModelCommand)
        {
            Initialization();

            SqliteDbContext sqliteDbContext = new SqliteDbContext();
            AllCashiers = sqliteDbContext.Cashiers.ToList();
            UpdateCurrentViewModelCommand = updateCurrentViewModelCommand;

            string? logoUrl = SettingsManager.Instance.GetPathToLogo();

            if (File.Exists(logoUrl))
            {
                Logo = new BitmapImage(new Uri(logoUrl));
            }
        }

        public List<CashierDB> AllCashiers { get; private set; }

        public ImageSource Logo
        {
            get { return _logo; }
            set
            {
                _logo = value;
                OnPropertyChange(nameof(Logo));
            }
        }
        public Visibility VisibilityBlack
        {
            get { return _visibilityBlack; }
            set
            {
                _visibilityBlack = value;
                OnPropertyChange(nameof(VisibilityBlack));
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChange(nameof(Message));
            }
        }
        public string CarNumber
        {
            get { return _carNumber; }
            set
            {
                if (value.Length == 10)
                {
                    CashierDB c;
                    if (value == CashierAdmin2.Id)
                    {
                        c = CashierAdmin2;
                    }
                    else
                    {
                        c = AllCashiers.Find(cashier => cashier.Id == value);
                    }

                    if (c != null)
                    {
#if CRNO
#else
                        Task.Run(() =>
                        {
                            SendPin();
                        });
#endif
                        AppStateParameter appStateParameter;
                        if (c.Type == CashierTypeEnumeration.Worker)
                        {
                            appStateParameter = new AppStateParameter(AppStateEnumerable.Sale, c);
                        }
                        else
                        {
                            appStateParameter = new AppStateParameter(AppStateEnumerable.Main, c);
                        }
                        UpdateCurrentViewModelCommand.Execute(appStateParameter);
                    }
                    else
                    {
                        Message = "Kasir ne postoji!";
                        CarNumber = string.Empty;
                    }
                    return;
                }
                _carNumber = value;
                OnPropertyChange(nameof(CarNumber));
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChange(nameof(Password));
            }
        }

        public ICommand UpdateCurrentViewModelCommand { get; set; }

        private void SendPin()
        {
            try
            {
                string? toPath = SettingsManager.Instance.GetInDirectory();

                if (string.IsNullOrEmpty(toPath))
                {
                    MessageBox.Show("Putanja za slanje PIN-a nije dobra",
                        "Greska u putanji PIN-a",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                if (File.Exists(@$"{toPath}\PIN.json"))
                {
                    File.Delete(@$"{toPath}\PIN.json");
                }

                File.Copy(_fromPath, @$"{toPath}\PIN.json", true);
            }
            catch
            {
                MessageBox.Show("Greska prilikom slanja PIN-a",
                    "Greska u PIN-u",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void KillApp()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }

        private bool ValidDateTime()
        {
            string esirId = SettingsManager.Instance.GetEsirId();

            if (string.IsNullOrWhiteSpace(esirId))
            {
                MessageBox.Show("CCS UniversalEsir nije pravilno aktiviran! Obratite se proizvođaču.", "Greška aktivacije", MessageBoxButton.OK, MessageBoxImage.Error);

                KillApp();
                return false;
            }

            try
            {
                _validTo = CCS_Fiscalization_ApiManager.Instance.GetValidTo(esirId).Result;

                if (_validTo.HasValue)
                {
                    SettingsManager.Instance.SetValidTo(_validTo.Value);
                }
            }
            catch
            {
                _validTo = SettingsManager.Instance.GetValidTo();
            }

            DateTime currentDateTime = DateTime.Now.Date;

            if (_validTo < currentDateTime)
            {
                MessageBox.Show("Istekla Vam je licenca! Obratite se proizvođaču da bi mogli dalje da radite.", "Istek aktivacije",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                KillApp();
                return false;
            }

            int day = _validTo.Value.Date.Subtract(currentDateTime).Days;

            if (day <= 10)
            {
                MessageBox.Show($"Vaša licenca ističe za {day} dana! Obratite se proizvođaču za produženje.", "Istek aktivacije",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return true;
        }
        private void Initialization()
        {
#if CRNO
            VisibilityBlack = Visibility.Hidden;
#else
            VisibilityBlack = Visibility.Visible;
#endif
            //if (SettingsManager.Instance.GetEnableCCS_Server())
            //{
            //    bool initializationCCS_Server = CCS_Fiscalization_ApiManager.Instance.Initialization().Result;

            //    if (!initializationCCS_Server)
            //    {
            //        MessageBox.Show("CCS ESIR nije pravilno aktiviran! Putanja do CCS SERVER-a ne postoji. Obratite se proizvođaču.",
            //            "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

            //        KillApp();
            //    }

            //    //if (!ValidDateTime())
            //    //{
            //    //    return;
            //    //}
            //    //bool firstValid = true;

            //    //if (_timer is null)
            //    //{
            //    //    _timer = new Timer(
            //    //        (e) =>
            //    //        {
            //    //            if (firstValid)
            //    //            {
            //    //                firstValid = false;
            //    //            }
            //    //            else
            //    //            {
            //    //                ValidDateTime();
            //    //            }
            //    //        },
            //    //        null,
            //    //        0,
            //    //        43200000);
            //    //}
            //}
            //if (_initializationTask is null)
            //{
            //    _initializationTask = Task.Run(async () =>
            //    {
            //        bool connectedWithLPFR = await ApiManager.Instance.Initialization();
            //    });
            //}
        }
    }
}