using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class RacunItem
    {
        public string RacunId { get; set; } = null!;
        public string ItemsId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }


        [JsonIgnore]
        public virtual Item Items { get; set; } = null!;

        [JsonIgnore]
        public virtual Racun Racun { get; set; } = null!;
    }
}
