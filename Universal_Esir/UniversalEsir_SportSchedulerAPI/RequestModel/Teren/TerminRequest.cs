namespace UniversalEsir_SportSchedulerAPI.RequestModel.Teren
{
    public class TerminRequest
    {
        public int TerenId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}
