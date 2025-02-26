namespace CcsSportScheduler_API.Models.Requests.Zaduzenje
{
    public class ZaduzenjeRequest
    {
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Placeno { get; set; }
        public decimal Otpis { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public string? Opis { get; set; }
        public int? NewTypeUser { get; set; }
        public int? Dan { get; set; }
        public int? Sat { get; set; }
        public int? Teren { get; set; }
    }
}
