namespace CcsSportScheduler_API.Models.Requests.Uplata
{
    public class UplataRequest
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Placeno { get; set; }
        public int TypeUplata { get; set; }
        public string? Description { get; set; }
    }
}
