﻿using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.Login;
using UniversalEsir_API;
using UniversalEsir_Common.Models.CCS_Server;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.Activation
{
    public class ActivationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private string[] _activationCodes;
        private MainViewModel _viewModel;

        public ActivationCommand(MainViewModel viewModel)
        {
            _activationCodes = File.ReadAllLines("ActivationCodes.cvs");
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public async void Execute(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da je 'Aktivacioni broj uređaja' ispravan?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool correctActivationCode = _activationCodes.Contains(parameter.ToString());

                if (correctActivationCode)
                {
                    string? esirId = SettingsManager.Instance.SetActivationCodeNumber(parameter.ToString());

                    if (string.IsNullOrEmpty(esirId))
                    {
                        return;
                    }

                    if (SettingsManager.Instance.GetEnableCCS_Server())
                    {
                        var resault = await CCS_Fiscalization_ApiManager.Instance.GetValidTo(esirId);

                        if (resault == null || !resault.HasValue)
                        {
                            bool insert = await CCS_Fiscalization_ApiManager.Instance.InsertPaymentPoint(new PaymentPoint
                            {
                                Id = esirId,
                                IdStore = parameter.ToString()
                            });

                            if (!insert)
                            {
                                MessageBox.Show("Komunikacija sa CCS SERVER-om nije dostupna! Proverite Vaš internet.", "Upozorenje!", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }
                    MessageBox.Show("Uspešno sačuvan 'Aktivacioni broj uređaja'!", "Uspešno!", MessageBoxButton.OK, MessageBoxImage.Information);
                    _viewModel.CurrentViewModel = new LoginViewModel(_viewModel.UpdateCurrentViewModelCommand);
                }
                else
                {
                    MessageBox.Show("Neispravan 'Aktivacioni broj uređaja'!\n" +
                        "Unesite 'Aktivacioni broj uređaja' koji ste dobili od proizvođača", "Greška!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}