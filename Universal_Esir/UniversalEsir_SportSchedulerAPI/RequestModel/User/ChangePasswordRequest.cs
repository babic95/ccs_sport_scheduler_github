using Newtonsoft.Json;

namespace UniversalEsir_SportSchedulerAPI.RequestModel.User
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
