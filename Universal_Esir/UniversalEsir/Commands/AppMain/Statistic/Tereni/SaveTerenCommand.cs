using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UniversalEsir.Models.AppMain.Statistic.Clanovi;
using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_SportSchedulerAPI.RequestModel.User;
using UniversalEsir_SportSchedulerAPI;
using UniversalEsir_SportSchedulerAPI.RequestModel.Teren;
using UniversalEsir_Logging;

namespace UniversalEsir.Commands.AppMain.Tereni
{
    public class SaveTerenCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ClanoviViewModel _currentViewModel;

        public SaveTerenCommand(ClanoviViewModel currentViewModel)
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
                if (string.IsNullOrEmpty(_currentViewModel.CurrentTeren.Name))
                {
                    MessageBox.Show("Niste popunili sva polja!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                TerenRequest terenRequest = new TerenRequest()
                {
                    KlubId = 1,
                    Name = _currentViewModel.CurrentTeren.Name
                };

                SportSchedulerAPI_Manager sportSchedulerAPI_Manager = new SportSchedulerAPI_Manager();

                if (sportSchedulerAPI_Manager.PostTerensAsync(terenRequest).Result)
                {
                    MessageBox.Show("Uspešno ste dodali novi teren!",
                                                "Uspeh",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Greška prilikom dodavanja novog terena!",
                        "Greška",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                _currentViewModel.CurrentWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error("SaveTerenCommand -> Greska prilikom cuvanja clana", ex);
                MessageBox.Show("Dogodila se greška!\nObratite se serviseru.",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
