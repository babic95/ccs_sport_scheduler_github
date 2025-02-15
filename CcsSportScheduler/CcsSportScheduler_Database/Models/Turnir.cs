using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class Turnir
    {
        public Turnir()
        {
            Terminturnirs = new HashSet<TerminTurnir>();
            Ucesnikturnirs = new HashSet<UcesnikTurnir>();
            Vestiturnirs = new HashSet<VestiTurnir>();
        }

        public string Id { get; set; } = null!;
        public int KlubId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; } = null!;
        public string? ImagesFolderName { get; set; }

        public virtual Klub Klub { get; set; } = null!;
        public virtual ICollection<TerminTurnir> Terminturnirs { get; set; }
        public virtual ICollection<UcesnikTurnir> Ucesnikturnirs { get; set; }
        public virtual ICollection<VestiTurnir> Vestiturnirs { get; set; }
    }
}
