using Newtonsoft.Json;

namespace UniversalEsir_SportSchedulerAPI.RequestModel.User
{
    public class LoginRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
