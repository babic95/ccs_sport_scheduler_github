using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Common.Models.Statistic.Driver
{
    public class IsporukaGlobal
    {
        public string Counter { get; set; }
        public string IsporukaName { get; set; }
        public DriverGlobal Driver { get; set; }
        public string TotalAmount { get; set; }
        public string DateIsporuke { get; set; }
        public string DateCreate { get; set; }
        public List<DriverInvoiceGlobal> DriverInvoiceGlobals { get; set; }
    }
}
