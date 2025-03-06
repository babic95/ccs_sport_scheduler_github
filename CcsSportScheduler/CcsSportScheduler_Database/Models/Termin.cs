using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class Termin
    {
        public Termin() 
        {
            Obavestenjas = new HashSet<Obavestenja>();
        }
        public string Id { get; set; } = null!;
        public int TerenId { get; set; }
        public int? UserId { get; set; }
        public int Type { get; set; }
        public string? TerminLigaId { get; set; }
        public string? TerminTurnirId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public decimal Price { get; set; }
        public decimal Placeno { get; set; }
        public DateTime DateRezervacije { get; set; }
        public decimal Otpis { get; set; }

        public virtual Teren Teren { get; set; } = null!;
        [JsonIgnore]
        public virtual TerminLiga? TerminLiga { get; set; }
        [JsonIgnore]
        public virtual TerminTurnir? TerminTurnir { get; set; }
        
        public virtual User? User { get; set; }
        [JsonIgnore]
        public virtual ICollection<Obavestenja> Obavestenjas { get; set; }
    }
}
