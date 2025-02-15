namespace UniversalEsir_SportSchedulerAPI.RequestModel.Uplata
{
    public class UplataRequest
    {
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public int TypeUplata { get; set; }
    }
}
