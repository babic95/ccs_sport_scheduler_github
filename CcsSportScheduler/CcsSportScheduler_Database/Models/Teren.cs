using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class Teren
    {
        public Teren()
        {
            Termins = new HashSet<Termin>();
        }

        public int Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;

        [JsonIgnore]
        public virtual Klub Klub { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Termin> Termins { get; set; }
    }
}
