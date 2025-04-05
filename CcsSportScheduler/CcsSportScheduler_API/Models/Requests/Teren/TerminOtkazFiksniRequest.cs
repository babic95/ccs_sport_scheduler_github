namespace CcsSportScheduler_API.Models.Requests.Teren
{
    public class TerminOtkazFiksniRequest
    {
        public int TerenId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
