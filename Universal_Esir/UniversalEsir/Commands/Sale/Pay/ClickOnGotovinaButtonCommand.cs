using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalEsir.ViewModels;
using UniversalEsir.ViewModels.Sale;

namespace UniversalEsir.Commands.Sale.Pay
{
    public class ClickOnGotovinaButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private PaySaleViewModel _viewModel;

        public ClickOnGotovinaButtonCommand(PaySaleViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            if (parameter is string)
            {
                string parameterString = (string)parameter;

                parameterString = parameterString.Replace(',', '.');

                if (_viewModel.FirstChangeGotovina &&
                    !parameterString.Contains("backspace"))
                {
                    _viewModel.FirstChangeGotovina = false;
                    _viewModel.Gotovina = parameterString;
                }
                else
                {
                    if (parameterString.Contains("backspace"))
                    {
                        if (_viewModel.Gotovina.Length == 1)
                        {
                            _viewModel.Gotovina = "1";
                            _viewModel.FirstChangeGotovina = true;
                        }
                        else
                        {
                            _viewModel.Gotovina = _viewModel.Gotovina.Substring(0, _viewModel.Gotovina.Length - 1);
                        }
                    }
                    //else if (parameterString.Contains("enter"))
                    //{
                    //    if()
                    //}
                    else if (parameterString.Contains(","))
                    {
                        _viewModel.Gotovina += ".0";
                    }
                    else if (parameterString.Contains("."))
                    {
                        _viewModel.Gotovina += ".0";
                    }
                    else
                    {
                        if (_viewModel.Gotovina.Length > 2 &&
                            _viewModel.Gotovina[_viewModel.Gotovina.Length - 1] == '0' &&
                            _viewModel.Gotovina[_viewModel.Gotovina.Length - 2] == '.' &&
                            parameterString != "0")
                        {
                            var q = _viewModel.Gotovina.Substring(0, _viewModel.Gotovina.Length - 1) + parameterString;
                            _viewModel.Gotovina = q;
                        }
                        else
                        {
                            _viewModel.Gotovina += parameterString;
                        }
                    }
                }
            }
        }
    }
}
