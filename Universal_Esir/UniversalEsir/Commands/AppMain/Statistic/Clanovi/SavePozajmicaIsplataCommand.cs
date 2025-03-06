using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_SportSchedulerAPI.RequestModel.Zaduzenje;
using UniversalEsir_SportSchedulerAPI;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Statistic.Clanovi
{
    public class SavePozajmicaIsplataCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public SavePozajmicaIsplataCommand(ClanoviViewModel currentViewModel)
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
                    _currentViewModel.CurrentZaduzenje.TotalAmount <= 0 ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentZaduzenje.Opis))
                {
                    MessageBox.Show("Niste popunili sva polja!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var result = MessageBox.Show("Da li ste sigurni da želite da sačuvate isplatu pozajmice?",
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
                        Type = (int)ZaduzenjeEnumeration.OtpisPozajmica,
                        Opis = _currentViewModel.CurrentZaduzenje.Opis
                    };

                    SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

                    if (sportSchedulerAPI_Manager.PostZaduzenjeAsync(zaduzenjeRequest).Result)
                    {
                        MessageBox.Show("Uspešno ste dodali isplatu pozajmice!",
                                                    "Uspeh",
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Greška prilikom dodavanja isplate pozajmice!",
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
                Log.Error("SavePozajmicaIsplataCommand -> Greska prilikom cuvanja isplate pozajmice", ex);
                MessageBox.Show("Dogodila se greška!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
