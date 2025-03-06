using System;
using System.Collections.Generic;

namespace UniversalEsir_Database.Models
{
    public partial class PaymentPlaceDB
    {
        public PaymentPlaceDB()
        {
            UnprocessedOrders = new HashSet<UnprocessedOrderDB>();
        }

        public int Id { get; set; }
        public int PartHallId { get; set; }
        public int UserId { get; set; }
        public decimal? LeftCanvas { get; set; }
        public decimal? TopCanvas { get; set; }
        public int? Type { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal AddPrice { get; set; }
        public string Name { get; set; }

        public virtual PartHallDB PartHall { get; set; } = null!;
        public virtual ICollection<UnprocessedOrderDB> UnprocessedOrders { get; set; }
    }
}
