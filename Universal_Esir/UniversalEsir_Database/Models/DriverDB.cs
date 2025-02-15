using UniversalEsir_Common.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Database.Models
{
    public partial class DriverDB
    {
        public DriverDB()
        {
            DriverInvoices = new HashSet<DriverInvoiceDB>();
        }
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Jmbg { get; set; }
        public string? Address { get; set; }
        public string? ContractNumber { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }

        public virtual ICollection<DriverInvoiceDB> DriverInvoices { get; set; }
    }
}
