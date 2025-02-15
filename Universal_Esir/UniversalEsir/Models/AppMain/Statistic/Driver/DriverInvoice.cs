using UniversalEsir.Models.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir.Models.AppMain.Statistic.Driver
{
    public class DriverInvoice : ObservableObject
    {
        private bool _isChecked;
        private Invoice _invoice;
        private Isporuka? _isporuka;
        
        public DriverInvoice(Invoice invoice, Isporuka? isporuka = null)
        {
            _isporuka = isporuka;
            Invoice = invoice;
            //IsChecked = true;
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChange(nameof(IsChecked));

                if (_isporuka != null &&
                    Invoice != null)
                {
                    if (value == false)
                    {
                        _isporuka.TotalAmount -= Invoice.TotalAmount;
                    }
                    else
                    {
                        _isporuka.TotalAmount += Invoice.TotalAmount;
                    }
                }
            }
        }
        public Invoice Invoice
        {
            get { return _invoice; }
            set
            {
                _invoice = value;
                OnPropertyChange(nameof(Invoice));
            }
        }
    }
}
