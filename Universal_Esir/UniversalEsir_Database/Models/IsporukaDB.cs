using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Database.Models
{
    public partial class IsporukaDB
    {
        public IsporukaDB()
        {
            DriverInvoices = new HashSet<DriverInvoiceDB>();
        }
        public string Id { get; set; } = null!;
        public int Counter { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DateIsporuka { get; set; }
        public decimal TotalAmount { get; set; }
        public virtual ICollection<DriverInvoiceDB> DriverInvoices { get; set; }
    }
}
