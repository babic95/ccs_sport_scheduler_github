using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Database.Models
{
    public partial class DriverInvoiceDB
    {
        public DriverInvoiceDB() 
        {
            Isporuke = new HashSet<IsporukaDB>();
        }
        public string InvoiceId { get; set; } = null!;
        public int DriverId { get; set; }
        public string? IsporukaId { get; set; }
        public virtual InvoiceDB Invoice { get; set; } = null!;
        public virtual DriverDB Driver { get; set; } = null!;
        public virtual ICollection<IsporukaDB> Isporuke { get; set; }
    }
}
