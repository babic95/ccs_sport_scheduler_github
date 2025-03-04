namespace CcsSportScheduler_API.Models.Requests.Teren
{
    public class TerminFiksniRequest
    {
        public int TerenId { get; set; }
        public int UserId { get; set; }
        public List<DateTime> Dates { get; set; }
        public int Zaduzi { get; set; }
    }
}
