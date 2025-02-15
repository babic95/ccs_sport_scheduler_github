using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.Norm
{
    public class OpenNormativWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public OpenNormativWindowCommand(InventoryStatusViewModel currentViewModel)
        {
            _currentViewModel = currentViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            if (!_currentViewModel.Norma.Any())
            {
                var norm = sqliteDbContext.Norms.Add(new NormDB());
                sqliteDbContext.SaveChanges();
                _currentViewModel.CurrentNorm = Convert.ToInt32(norm.Property("Id").CurrentValue);
            }

            AddNormWindow addNormWindow = new AddNormWindow(_currentViewModel);
            addNormWindow.ShowDialog();
        }
    }
}