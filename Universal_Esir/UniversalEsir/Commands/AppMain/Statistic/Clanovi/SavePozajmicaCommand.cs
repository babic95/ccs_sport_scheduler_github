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
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Statistic.Clanovi
{
    public class SavePozajmicaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public SavePozajmicaCommand(ClanoviViewModel currentViewModel)
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
                    _currentViewModel.CurrentUplata.Date == null ||
                    _currentViewModel.CurrentUplata.TotalAmount <= 0 ||
                    string.IsNullOrEmpty(_currentViewModel.CurrentUplata.Description))
                {
                    MessageBox.Show("Niste popunili sva polja!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                UplataRequest uplataRequest = new UplataRequest()
                {
                    UserId = _currentViewModel.CurrentClan.Id,
                    Date = _currentViewModel.CurrentUplata.Date,
                    TotalAmount = _currentViewModel.CurrentUplata.TotalAmount,
                    TypeUplata = (int)UplataEnumeration.Pozajmica,
                    Description = _currentViewModel.CurrentUplata.Description
                };

                SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

                if (sportSchedulerAPI_Manager.PostUplataAsync(uplataRequest).Result)
                {
                    MessageBox.Show("Uspešno ste dodali pozajmicu!",
                                                "Uspeh",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Greška prilikom dodavanja pozajmice!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                _currentViewModel.CurrentWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error("SavePozajmicaCommand -> Greska prilikom cuvanja pozajmice", ex);
                MessageBox.Show("Dogodila se greška!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
