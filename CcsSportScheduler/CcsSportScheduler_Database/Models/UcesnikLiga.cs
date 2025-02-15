using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class UcesnikLiga
    {
        public int UserId { get; set; }
        public string LigaId { get; set; } = null!;
        public DateTime DatumPrijave { get; set; }

        public virtual Liga Liga { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
