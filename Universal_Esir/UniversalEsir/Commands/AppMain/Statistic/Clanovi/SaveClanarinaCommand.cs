using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_SportSchedulerAPI.RequestModel.Uplata;
using UniversalEsir_SportSchedulerAPI;
using UniversalEsir_Logging;
using UniversalEsir_SportSchedulerAPI.RequestModel.Zaduzenje;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;

namespace UniversalEsir.Commands.AppMain.Statistic.Clanovi
{
    public class SaveClanarinaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public SaveClanarinaCommand(ClanoviViewModel currentViewModel)
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
                if (_currentViewModel.CurrentClan == null ||
                    _currentViewModel.CurrentZaduzenje.Date == null ||
                    (_currentViewModel.CurrentZaduzenje.ClanType != ClanEnumeration.Turnir && _currentViewModel.CurrentZaduzenje.TotalAmount <= 0) ||
                    _currentViewModel.CurrentZaduzenje.ClanType == null)
                {
                    MessageBox.Show("Niste popunili sva polja!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("Da li ste sigurni da želite da sačuvate članarinu?",
                    "Upozorenje",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ZaduzenjeRequest zaduzenjeRequest = new ZaduzenjeRequest()
                    {
                        UserId = _currentViewModel.CurrentClan.Id,
                        Date = _currentViewModel.CurrentZaduzenje.Date,
                        TotalAmount = _currentViewModel.CurrentZaduzenje.TotalAmount,
                        Type = (int)ZaduzenjeEnumeration.Clanarina,
                        NewTypeUser = (int)_currentViewModel.CurrentZaduzenje.ClanType,
                        Dan = (int)_currentViewModel.CurrentDan,
                        Sat = _currentViewModel.CurrentSatiTermina.Sat,
                        Teren = _currentViewModel.CurrentZaduzenje.Teren,
                    };

                    SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

                    if (sportSchedulerAPI_Manager.PostZaduzenjeAsync(zaduzenjeRequest).Result)
                    {
                        MessageBox.Show("Uspešno ste dodali članarinu!",
                                                    "Uspeh",
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom dodavanja članarine!",
                            "Greška",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }

                    _currentViewModel.CurrentWindow.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("SaveClanarinaCommand -> Greska prilikom cuvanja clanarine", ex);
                MessageBox.Show("Dogodila se greška!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
