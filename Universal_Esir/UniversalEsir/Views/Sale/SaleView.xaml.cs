using UniversalEsir.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UniversalEsir.Views.Sale
{
    /// <summary>
    /// Interaction logic for SaleView.xaml
    /// </summary>
    public partial class SaleView : UserControl
    {
        private SaleViewModel _viewModel;

        public SaleView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            sifra.Focus();
            sifra.SelectAll();
            _viewModel = DataContext as SaleViewModel;
        }

        private void sifra_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                name.Focus();
            }
        }
        private void name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                popust.Focus();
                popust.SelectAll();
            }
        }

        private void popust_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                kolicina.Focus();
                kolicina.SelectAll();
            }
        }

        private void kolicina_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sifra.Focus();
                SaleViewModel saleViewModel = (SaleViewModel)DataContext;
                saleViewModel.FindItemInDB();
            }
        }
        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        { 
            if(sender is TextBox textBox1)
            {
                int a = 2;
            }

            if (sender is TextBox textBox && textBox.Tag is string tag &&
                _viewModel != null) 
            {
                _viewModel.FocusedTextBox = tag; 
            }
        }
    }
}
