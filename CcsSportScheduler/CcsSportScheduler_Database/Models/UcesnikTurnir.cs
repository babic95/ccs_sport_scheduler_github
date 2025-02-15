using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class UcesnikTurnir
    {
        public int UserId { get; set; }
        public string TurnirId { get; set; } = null!;
        public DateTime DatumPrijave { get; set; }

        public virtual Turnir Turnir { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
