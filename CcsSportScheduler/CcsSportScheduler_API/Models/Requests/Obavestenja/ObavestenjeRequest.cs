namespace CcsSportScheduler_API.Models.Requests.Obavestenja
{
    public class ObavestenjeRequest
    {
        public int UserId { get; set; }
        public string Description { get; set; } = null!;
        public string? TerminId { get; set; }
    }
}
