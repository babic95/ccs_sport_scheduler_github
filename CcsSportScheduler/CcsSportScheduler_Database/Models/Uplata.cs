using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class Uplata
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Razduzeno { get; set; }
        public DateTime Date { get; set; }
        public int TypeUplata { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
