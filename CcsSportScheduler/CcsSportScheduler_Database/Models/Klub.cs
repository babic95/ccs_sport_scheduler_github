using System;
using System.Collections.Generic;

namespace CcsSportScheduler_Database.Models
{
    public partial class Klub
    {
        public Klub()
        {
            Items = new HashSet<Item>();
            Ligas = new HashSet<Liga>();
            Naplataterminas = new HashSet<NaplataTermina>();
            Popustiterminas = new HashSet<PopustiTermina>();
            Terens = new HashSet<Teren>();
            Turnirs = new HashSet<Turnir>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Pib { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? Number { get; set; }
        public string? Email { get; set; }
        public int DanaValute { get; set; }

        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Liga> Ligas { get; set; }
        public virtual ICollection<NaplataTermina> Naplataterminas { get; set; }
        public virtual ICollection<PopustiTermina> Popustiterminas { get; set; }
        public virtual ICollection<Teren> Terens { get; set; }
        public virtual ICollection<Turnir> Turnirs { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
