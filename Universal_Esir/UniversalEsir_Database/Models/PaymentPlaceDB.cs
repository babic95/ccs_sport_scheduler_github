using System;
using System.Collections.Generic;

namespace UniversalEsir_Database.Models
{
    public partial class PaymentPlaceDB
    {
        public int Id { get; set; }
        public int PartHallId { get; set; }
        public decimal? LeftCanvas { get; set; }
        public decimal? TopCanvas { get; set; }
        public int? Type { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public virtual PartHallDB PartHall { get; set; } = null!;
        public virtual ICollection<UnprocessedOrderDB> UnprocessedOrders { get; set; }
    }
}
