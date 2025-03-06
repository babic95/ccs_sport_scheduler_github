﻿using System;
using System.Collections.Generic;

namespace UniversalEsir_Database.Models
{
    public partial class OrderDB
    {
        public int PaymentPlaceId { get; set; }
        public string InvoiceId { get; set; } = null!;
        public string CashierId { get; set; } = null!;

        public virtual CashierDB Cashier { get; set; } = null!;
        public virtual InvoiceDB Invoice { get; set; } = null!;
    }
}
