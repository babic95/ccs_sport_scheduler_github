using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public int KlubId { get; set; }
        public string FullName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int Type { get; set; }
        public DateTime Birthday { get; set; }
        public string Contact { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Jmbg { get; set; } = null!;
        public int FreeTermin { get; set; }
    }
}
