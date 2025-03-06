using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class Obavestenja
    {
        public string Id { get; set; } = null!;
        public string? TerminId { get; set; }
        public int UserId { get; set; }
        public byte[] Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Seen { get; set; }
        public int PrvoSlanje { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        [JsonIgnore]
        public virtual Termin? Termin { get; set; }
    }
}
