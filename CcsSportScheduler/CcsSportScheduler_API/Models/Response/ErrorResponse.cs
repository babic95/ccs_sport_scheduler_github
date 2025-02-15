using CcsSportScheduler_API.Enumeration;
using Microsoft.AspNetCore.Mvc;

namespace CcsSportScheduler_API.Models.Response
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public ErrorEnumeration Code { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
