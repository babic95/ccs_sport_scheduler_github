using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir_Printer.Models
{
    internal enum Type
    {
        Items = 0,
        Sirovine = 1,
        End = 2,
        ItemsFix = 3,
        Copy = 4,
    }

    internal class MorePage
    {
        public Type Type { get; set; }
        public int Index { get; set; }
    }
}
