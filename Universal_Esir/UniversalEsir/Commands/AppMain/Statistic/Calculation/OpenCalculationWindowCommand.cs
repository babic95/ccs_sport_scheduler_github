using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Calculation
{
    public class OpenCalculationWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private CalculationViewModel _currentViewModel;

        public OpenCalculationWindowCommand(CalculationViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //SqliteDbContext sqliteDbContext = new SqliteDbContext();

            //var norm = sqliteDbContext.Norms.Add(new NormDB());
            //sqliteDbContext.SaveChanges();
            //_currentViewModel.CurrentNorm = Convert.ToInt32(norm.Property("Id").CurrentValue);

            AddCaclulationWindow addCaclulationWindow = new AddCaclulationWindow(_currentViewModel);
            addCaclulationWindow.ShowDialog();

            _currentViewModel.Window = addCaclulationWindow;
        }
    }
}