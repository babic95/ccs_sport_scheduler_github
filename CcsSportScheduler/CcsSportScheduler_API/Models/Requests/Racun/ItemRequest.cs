namespace CcsSportScheduler_API.Models.Requests.Racun
{
    public class ItemRequest
    {
        public string Id { get; set; } = null!;
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
