using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class TerminLiga
    {
        public TerminLiga()
        {
            Termins = new HashSet<Termin>();
        }

        public string Id { get; set; } = null!;
        public string LigaId { get; set; } = null!;
        public int Type { get; set; }

        public virtual Liga Liga { get; set; } = null!;
        public virtual ICollection<Termin> Termins { get; set; }
    }
}
