using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class PopustiTermina
    {
        public int Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Popust { get; set; }
        public int TypeUser { get; set; }

        public virtual Klub Klub { get; set; } = null!;
    }
}
