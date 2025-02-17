using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class NaplataTermina
    {
        public int Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        [JsonIgnore]
        public virtual Klub Klub { get; set; } = null!;
    }
}
