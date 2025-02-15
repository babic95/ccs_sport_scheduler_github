namespace UniversalEsir_SportSchedulerAPI.RequestModel.Klub
{
    public class KlubRequest
    {
        public int? Id { get; set; }
        public string Pib { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? Number { get; set; }
        public string? Email { get; set; }
    }
}
