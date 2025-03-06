namespace UniversalEsir_SportSchedulerAPI.RequestModel.Racun
{
    public class ItemRacunRequest
    {
        public string ItemsId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
