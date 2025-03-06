﻿using UniversalEsir.ViewModels.AppMain.Statistic;
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

namespace UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic
{
    /// <summary>
    /// Interaction logic for AddCaclulationWindow.xaml
    /// </summary>
    public partial class AddCaclulationWindow : Window
    {
        public AddCaclulationWindow(CalculationViewModel calculationViewModel)
        {
            InitializeComponent();

            DataContext = calculationViewModel;
            calculationViewModel.Window = this;
        }
    }
}
