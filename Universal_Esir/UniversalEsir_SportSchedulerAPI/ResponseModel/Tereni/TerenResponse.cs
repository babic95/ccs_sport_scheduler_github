using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.Tereni
{
    public class TerenResponse
    {
        public int Id { get; set; }
        public int KlubId { get; set; }
        public string Name { get; set; } = null!;
    }
}
