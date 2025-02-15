using Newtonsoft.Json;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.FinancialCard
{
    public class FinancialCardResponse
    {
        [JsonProperty("totalZaduzenje")]
        public decimal TotalZaduzenje { get; set; }
        [JsonProperty("totalRazduzenje")]
        public decimal TotalRazduzenje { get; set; }
        [JsonProperty("items")]
        public List<FinancialCardItemResponse> Items { get; set; }
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
    }
}
