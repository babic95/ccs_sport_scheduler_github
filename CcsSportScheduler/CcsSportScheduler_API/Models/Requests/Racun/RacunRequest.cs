namespace CcsSportScheduler_API.Models.Requests.Racun
{
    public class RacunRequest
    {
        public int UserId { get; set; }
        public DateTime? Date { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<ItemRacunRequest> Items { get; set; }
    }
}
