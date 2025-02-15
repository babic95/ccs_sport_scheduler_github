using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class TerminTurnir
    {
        public TerminTurnir()
        {
            Termins = new HashSet<Termin>();
        }

        public string Id { get; set; } = null!;
        public string TurnirId { get; set; } = null!;
        public int Type { get; set; }

        public virtual Turnir Turnir { get; set; } = null!;
        public virtual ICollection<Termin> Termins { get; set; }
    }
}
