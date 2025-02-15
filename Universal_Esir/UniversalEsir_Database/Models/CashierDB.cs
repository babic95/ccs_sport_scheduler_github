﻿using UniversalEsir_Common.Enums;
using System;
using System.Collections.Generic;

namespace UniversalEsir_Database.Models
{
    public partial class CashierDB
    {
        public CashierDB()
        {
            Orders = new HashSet<OrderDB>();
            SmartCards = new HashSet<SmartCardDB>();
            Calculations = new HashSet<CalculationDB>();
            UnprocessedOrders = new HashSet<UnprocessedOrderDB>();
        }

        public string Id { get; set; } = null!;
        public CashierTypeEnumeration Type { get; set; }
        public string Name { get; set; } = null!;
        public string? Jmbg { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }

        public virtual ICollection<UnprocessedOrderDB> UnprocessedOrders { get; set; }
        public virtual ICollection<OrderDB> Orders { get; set; }
        public virtual ICollection<SmartCardDB> SmartCards { get; set; }
        public virtual ICollection<CalculationDB> Calculations { get; set; }
    }
}
