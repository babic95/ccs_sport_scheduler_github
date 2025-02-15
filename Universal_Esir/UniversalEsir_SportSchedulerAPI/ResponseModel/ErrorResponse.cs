using UniversalEsir_SportSchedulerAPI.Enumeration;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public ErrorEnumeration Code { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
