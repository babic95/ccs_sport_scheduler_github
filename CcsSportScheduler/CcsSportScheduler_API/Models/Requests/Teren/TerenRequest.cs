namespace CcsSportScheduler_API.Models.Requests.Teren
{
    public class TerenRequest
    {
        public int? Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
    }
}
