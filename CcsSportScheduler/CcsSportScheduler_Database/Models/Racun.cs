using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class Racun
    {
        public Racun()
        {
            Racunitems = new HashSet<RacunItem>();
        }

        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Placeno { get; set; }
        public decimal Otpis { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        //[JsonIgnore]
        public virtual ICollection<RacunItem> Racunitems { get; set; }
    }
}
