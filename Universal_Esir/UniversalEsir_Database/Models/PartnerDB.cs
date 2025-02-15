﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Database.Models
{
    public partial class PartnerDB
    {
        public int Id { get; set; }
        public string Pib { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? ContractNumber { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Mb { get; set; }
    }
}
