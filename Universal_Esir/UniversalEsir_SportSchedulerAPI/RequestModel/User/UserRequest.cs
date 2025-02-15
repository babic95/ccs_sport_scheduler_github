namespace UniversalEsir_SportSchedulerAPI.RequestModel.User
{
    public class UserRequest
    {
        public int? Id { get; set; }
        public int? KlubId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Password { get; set; }
        public string? Username { get; set; }
        public int? Type { get; set; }
        public DateTime Birthday { get; set; }
        public string Contact { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Jmbg { get; set; } = null!;
    }
}
