namespace UniversalEsir_SportSchedulerAPI.RequestModel.Klub
{
    public class PopustiTerminaRequest
    {
        public int? Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Popust { get; set; }
        public int TypeUser { get; set; }
    }
}
