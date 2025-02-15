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
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal Price { get; set; }
        public int Vikend { get; set; }

        [JsonIgnore]
        public virtual Klub Klub { get; set; } = null!;
    }
}
