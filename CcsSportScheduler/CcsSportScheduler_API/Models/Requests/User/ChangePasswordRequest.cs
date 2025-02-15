using Newtonsoft.Json;

namespace CcsSportScheduler_API.Models.Requests.User
{
    public class ChangePasswordRequest
    {
        [JsonProperty("idUser")]
        public int IdUser { get; set; }
        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }
        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }
}
