using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CcsSportScheduler_Database.Models
{
    public class Zaduzenje
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public string? Opis { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Placeno { get; set; }
        public decimal Otpis { get; set; }
        public int Type { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
