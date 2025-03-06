using UniversalEsir.ViewModels.AppMain.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.Commands.AppMain.Statistic.InventoryStatus
{
    public class SaveGroupItemsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private InventoryStatusViewModel _currentViewModel;

        public SaveGroupItemsCommand(InventoryStatusViewModel currentViewModel)
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
                if (_currentViewModel.CurrentGroupItems != null)
                {
                    if(_currentViewModel.CurrentSupergroup == null)
                    {
                        MessageBox.Show("Grupa artikala mora da pripada nekoj nadgrupi!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                    SqliteDbContext sqliteDbContext = new SqliteDbContext();

                    var groupItems = sqliteDbContext.ItemGroups.FirstOrDefault(group => group.Id == _currentViewModel.CurrentGroupItems.Id);

                    if (groupItems != null)
                    {
                        var result = MessageBox.Show("Da li zaista želite da sačuvate izmene grupe artikala?",
                            "Izmena grupe artikala",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            groupItems.IdSupergroup = _currentViewModel.CurrentSupergroup.Id;
                            groupItems.Name = _currentViewModel.CurrentGroupItems.Name;
                            sqliteDbContext.ItemGroups.Update(groupItems);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("Da li zaista želite da sačuvate novu grupu artikala?",
                            "Nova grupa artikala",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            ItemGroupDB itemGroupDB = new ItemGroupDB()
                            {
                                IdSupergroup = _currentViewModel.CurrentSupergroup.Id,
                                Name = _currentViewModel.CurrentGroupItems.Name
                            };
                            sqliteDbContext.ItemGroups.Add(itemGroupDB);
                        }
                        else
                        {
                            return;
                        }
                    }
                    sqliteDbContext.SaveChanges();

                    MessageBox.Show("Uspešno obavljeno!",
                            "Uspešno",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    _currentViewModel.Window.Close();
                }
            }
            catch
            {
                MessageBox.Show("Greška prilikom kreiranja ili izmene grupe artikala!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}