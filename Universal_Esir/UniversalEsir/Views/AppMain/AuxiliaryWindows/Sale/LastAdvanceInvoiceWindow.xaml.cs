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
using UniversalEsir.ViewModels.Sale;

namespace UniversalEsir.Views.AppMain.AuxiliaryWindows.Sale
{
    /// <summary>
    /// Interaction logic for LastAdvanceInvoiceWindow.xaml
    /// </summary>
    public partial class LastAdvanceInvoiceWindow : Window
    {
        public LastAdvanceInvoiceWindow(PaySaleViewModel paySaleViewModel)
        {
            InitializeComponent();

            DataContext = paySaleViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
