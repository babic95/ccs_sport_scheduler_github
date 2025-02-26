using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_SportSchedulerAPI.ResponseModel.User;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.Uplate
{
    public class UplataResponse
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Razduzeno { get; set; }
        public DateTime Date { get; set; }
        public int TypeUplata { get; set; }
        public string Description { get; set; }
        public UserResponse User { get; set; } = null!;
    }
}
