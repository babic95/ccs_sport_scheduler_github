namespace UniversalEsir_SportSchedulerAPI.RequestModel.Racun
{
    public class RacunRequest
    {
        public int UserId { get; set; }
        public int Type { get; set; }
        public DateTime? Date { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<ItemRacunRequest> Items { get; set; }
        public decimal Placeno { get; set; }
    }
}
