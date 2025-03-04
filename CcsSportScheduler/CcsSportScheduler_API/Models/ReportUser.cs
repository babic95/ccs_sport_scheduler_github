namespace CcsSportScheduler_API.Models
{
    public class ReportUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public decimal TotalZaduzenje { get; set; }
        public decimal TotalRazduzenje { get; set; }
        public decimal TotalOtpis { get; set; }
        public decimal TotalSaldo { get; set; }
        public List<RportUserItems> Items { get; set; }
    }
}
