using CcsSportScheduler_API.Enumeration;
using Newtonsoft.Json;

namespace CcsSportScheduler_API.Models.Response.FinancialCard
{
    public class FinancialCardItemResponse
    {
        [JsonProperty("type")]
        public FinancialCardTypeEnumeration Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
        [JsonProperty("zaduzenje")]
        public decimal Zaduzenje { get; set; }
        [JsonProperty("razduzenje")]
        public decimal Razduzenje { get; set; }
        [JsonProperty("pretplata")]
        public decimal Pretplata { get; set; }
        [JsonProperty("otpis")]
        public decimal Otpis { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }
}
