using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class VestiLiga
    {
        public string Id { get; set; } = null!;
        public string LigaId { get; set; } = null!;
        public byte[] Description { get; set; } = null!;

        public virtual Liga Liga { get; set; } = null!;
    }
}
