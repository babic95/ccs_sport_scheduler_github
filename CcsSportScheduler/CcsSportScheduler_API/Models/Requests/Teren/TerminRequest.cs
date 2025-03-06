namespace CcsSportScheduler_API.Models.Requests.Teren
{
    public class TerminRequest
    {
        public int TerenId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int Zaduzi { get; set; }
    }
}
