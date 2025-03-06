using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class VestiTurnir
    {
        public string Id { get; set; } = null!;
        public string TurnirId { get; set; } = null!;
        public byte[] Description { get; set; } = null!;

        public virtual Turnir Turnir { get; set; } = null!;
    }
}
