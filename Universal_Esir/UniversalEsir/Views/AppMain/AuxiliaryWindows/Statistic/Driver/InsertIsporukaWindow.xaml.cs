using UniversalEsir.ViewModels.AppMain.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver
{
    /// <summary>
    /// Interaction logic for InsertIsporukaWindow.xaml
    /// </summary>
    public partial class InsertIsporukaWindow : Window
    {
        public InsertIsporukaWindow(DriverViewModel driverViewModel)
        {
            InitializeComponent();
            DataContext = driverViewModel;
        }
    }
}
