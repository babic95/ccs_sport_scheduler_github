using System;
using System.Collections.Generic;

namespace UniversalEsir_Database.Models
{
    public partial class ItemDB
    {
        public ItemDB()
        {
            Procurements = new HashSet<ProcurementDB>();
            ItemInNorms = new HashSet<ItemInNormDB>();
            ItemsInUnprocessedOrder = new HashSet<ItemInUnprocessedOrderDB>();
            CalculationItems = new HashSet<CalculationItemDB>();
            ItemsNivelacija = new HashSet<ItemNivelacijaDB>();
        }

        public string Id { get; set; } = null!;
        public int IdItemGroup { get; set; }
        public string? Barcode { get; set; }
        public int? IdNorm { get; set; }
        public string Name { get; set; } = null!;
        public decimal SellingUnitPrice { get; set; }
        public decimal? InputUnitPrice { get; set; }
        public string Label { get; set; } = null!;
        public string Jm { get; set; } = null!;
        public decimal TotalQuantity { get; set; }
        public decimal? AlarmQuantity { get; set; }
        public string DisplayName 
        {
            get
            {
                return $"{Name} - {SellingUnitPrice}din - {TotalQuantity}{Jm}";
            }
        }

        public virtual ItemGroupDB ItemGroupNavigation { get; set; } = null!;
        public virtual NormDB Norm { get; set; } = null!;
        public virtual ICollection<ProcurementDB> Procurements { get; set; }
        public virtual ICollection<ItemInNormDB> ItemInNorms { get; set; }
        public virtual ICollection<ItemInUnprocessedOrderDB> ItemsInUnprocessedOrder { get; set; }
        public virtual ICollection<CalculationItemDB> CalculationItems { get; set; }
        public virtual ICollection<ItemNivelacijaDB> ItemsNivelacija { get; set; }

    }
}
