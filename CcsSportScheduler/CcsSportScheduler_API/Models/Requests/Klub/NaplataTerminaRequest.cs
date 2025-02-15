namespace CcsSportScheduler_API.Models.Requests.Klub
{
    public class NaplataTerminaRequest
    {
        public int? Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal Price { get; set; }
        public int Vikend { get; set; }
    }
}
