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
using UniversalEsir.ViewModels.AppMain.Statistic;

namespace UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Clanovi
{
    /// <summary>
    /// Interaction logic for AddNewOtpisWindow.xaml
    /// </summary>
    public partial class AddNewOtpisWindow : Window
    {
        public AddNewOtpisWindow(ClanoviViewModel clanoviViewModel)
        {
            InitializeComponent();
            DataContext = clanoviViewModel;
        }
    }
}
