using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class Item
    {
        public Item()
        {
            Racunitems = new HashSet<RacunItem>();
        }

        public string Id { get; set; } = null!;
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        public virtual Klub Klub { get; set; } = null!;
        public virtual ICollection<RacunItem> Racunitems { get; set; }
    }
}
