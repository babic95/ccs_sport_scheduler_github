using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.Racuni
{
    public class RacunItemResponse
    {
        public string RacunId { get; set; } = null!;
        public string ItemsId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
