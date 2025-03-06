using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class Liga
    {
        public Liga()
        {
            Terminligas = new HashSet<TerminLiga>();
            Ucesnikligas = new HashSet<UcesnikLiga>();
            Vestiligas = new HashSet<VestiLiga>();
        }

        public string Id { get; set; } = null!;
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImagesFolderName { get; set; }

        public virtual Klub Klub { get; set; } = null!;
        public virtual ICollection<TerminLiga> Terminligas { get; set; }
        public virtual ICollection<UcesnikLiga> Ucesnikligas { get; set; }
        public virtual ICollection<VestiLiga> Vestiligas { get; set; }
    }
}
