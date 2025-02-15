﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Database.Models
{
    public class SupergroupDB
    {
        public SupergroupDB()
        {
            ItemGroups = new HashSet<ItemGroupDB>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<ItemGroupDB> ItemGroups { get; set; }
        
    }
}
