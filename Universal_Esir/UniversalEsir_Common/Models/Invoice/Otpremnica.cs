using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Common.Models.Invoice
{
    public class Otpremnica : InvoiceRequest
    {
        public string? Porudzbenica { get; set; }
        public string InvoiceNumberResult { get; set; }
        public DateTime SdcDateTime { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
