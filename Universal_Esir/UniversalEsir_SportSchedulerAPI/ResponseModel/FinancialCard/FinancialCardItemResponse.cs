using UniversalEsir_SportSchedulerAPI.Enumeration;
using Newtonsoft.Json;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.FinancialCard
{
    public class FinancialCardItemResponse
    {
        [JsonProperty("type")]
        public FinancialCardTypeEnumeration Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("zaduzenje")]
        public decimal Zaduzenje { get; set; }
        [JsonProperty("razduzenje")]
        public decimal Razduzenje { get; set; }
        [JsonProperty("otpis")]
        public decimal Otpis { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }
}
