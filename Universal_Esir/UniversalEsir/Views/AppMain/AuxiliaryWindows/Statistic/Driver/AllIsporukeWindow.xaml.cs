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

namespace UniversalEsir.Views.AppMain.AuxiliaryWindows.Statistic.Driver
{
    /// <summary>
    /// Interaction logic for AllIsporukeWindow.xaml
    /// </summary>
    public partial class AllIsporukeWindow : Window
    {
        public AllIsporukeWindow(DriverViewModel driverViewModel)
        {
            InitializeComponent();
            DataContext = driverViewModel;
        }
    }
}
