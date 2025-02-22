﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Clanovi;
using UniversalEsir_Database;
using UniversalEsir_Logging;
using UniversalEsir_SportSchedulerAPI;
using UniversalEsir_SportSchedulerAPI.RequestModel.User;
using UniversalEsir_SportSchedulerAPI.ResponseModel.User;

namespace UniversalEsir.Commands.AppMain.Clanovi
{
    internal class SaveClanCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public SaveClanCommand(ClanoviViewModel currentViewModel)
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
                if(string.IsNullOrEmpty(_currentViewModel.CurrentClan.FullName) ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentClan.Contact) ||
                    _currentViewModel.CurrentClan.Birthday == null ||
                    _currentViewModel.CurrentClan.Type == null ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentClan.Email) ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentClan.Jmbg) ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentClan.Password) ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentClan.Username))
                {
                    MessageBox.Show("Niste popunili sva polja!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                UserRequest userRequest = new UserRequest()
                {
                    KlubId = 1,
                    FullName = _currentViewModel.CurrentClan.FullName,
                    Contact = _currentViewModel.CurrentClan.Contact,
                    Birthday = _currentViewModel.CurrentClan.Birthday,
                    Email = _currentViewModel.CurrentClan.Email,
                    Jmbg = _currentViewModel.CurrentClan.Jmbg,
                    Password = _currentViewModel.CurrentClan.Password,
                    Type = (int)_currentViewModel.CurrentClan.Type,
                    Username = _currentViewModel.CurrentClan.Username
                };

                SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

                if (_currentViewModel.CurrentClan.Id == -1)
                {
                    if (sportSchedulerAPI_Manager.PostUsersAsync(userRequest).Result)
                    {
                        MessageBox.Show("Uspešno ste dodali novog clana!",
                                                    "Uspeh",
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom dodavanja novog clana!",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }
                }
                else
                {
                    userRequest.Id = _currentViewModel.CurrentClan.Id;

                    if (sportSchedulerAPI_Manager.PutUsersAsync(userRequest).Result)
                    {
                        MessageBox.Show("Uspešno ste izmenili clana!",
                                                    "Uspeh",
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom izmene clana!",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }
                }

                _currentViewModel.Clanovi = new ObservableCollection<Clan>();
                var clanovi = sportSchedulerAPI_Manager.GetUsersAsync().Result;

                if (clanovi == null)
                {
                    MessageBox.Show("Greška prilikom učitavanja korisnika!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                if (clanovi.Any())
                {
                    foreach (var clan in clanovi)
                    {
                        _currentViewModel.Clanovi.Add(new Clan(clan));
                    }
                }

                _currentViewModel.CurrentWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error("SaveClanCommand -> Greska prilikom cuvanja clana", ex);
                MessageBox.Show("Dogodila se greška!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
