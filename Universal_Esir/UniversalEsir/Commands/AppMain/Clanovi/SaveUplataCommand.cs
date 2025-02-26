using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_SportSchedulerAPI.RequestModel.Teren;
using UniversalEsir_SportSchedulerAPI;
using UniversalEsir_SportSchedulerAPI.RequestModel.Uplata;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Clanovi
{
    public class SaveUplataCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public SaveUplataCommand(ClanoviViewModel currentViewModel)
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
                if (_currentViewModel.CurrentClan == null)
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
                    TypeUplata = (int)_currentViewModel.CurrentUplata.TypeUplata
                };

                SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

                if (sportSchedulerAPI_Manager.PostUplataAsync(uplataRequest).Result)
                {
                    MessageBox.Show("Uspešno ste dodali uplatu!",
                                                "Uspeh",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Greška prilikom dodavanja uplate!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                _currentViewModel.CurrentWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error("SaveUplataCommand -> Greska prilikom cuvanja uplate", ex);
                MessageBox.Show("Dogodila se greška!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
