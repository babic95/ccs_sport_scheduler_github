using Newtonsoft.Json;

namespace CcsSportScheduler_API.Models.Requests.User
{
    public class LoginRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
