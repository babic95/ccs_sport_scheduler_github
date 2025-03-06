using Newtonsoft.Json;

namespace CcsSportScheduler_API.Models.Response.FinancialCard
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
