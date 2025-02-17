namespace CcsSportScheduler_API.Models.Requests.Klub
{
    public class NaplataTerminaRequest
    {
        public int Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
