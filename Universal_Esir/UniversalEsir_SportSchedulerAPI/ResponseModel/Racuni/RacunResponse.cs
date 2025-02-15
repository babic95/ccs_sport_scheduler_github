using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.Racuni
{
    public class RacunResponse
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Placeno { get; set; }
        public decimal Otpis { get; set; }

        public virtual ICollection<RacunItemResponse> Racunitems { get; set; }
    }
}
