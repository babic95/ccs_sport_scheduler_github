namespace CcsSportScheduler_API.Models.Requests.Report
{
    public class ReportRequest
    {
        public int UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
