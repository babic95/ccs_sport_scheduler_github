namespace UniversalEsir_SportSchedulerAPI.RequestModel.User
{
    public class UserRequest
    {
        public int? Id { get; set; }
        public int? KlubId { get; set; }
        public string FullName { get; set; }
        public string? Password { get; set; }
        public string? Username { get; set; }
        public int? Type { get; set; }
        public int Year { get; set; }
        public int Pol { get; set; }
        public string Contact { get; set; }
        public string? Email { get; set; }
        public string Jmbg { get; set; }
    }
}
