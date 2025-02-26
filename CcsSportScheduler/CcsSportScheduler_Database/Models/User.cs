using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CcsSportScheduler_Database.Models
{
    public partial class User
    {
        public User()
        {
            Obavestenjas = new HashSet<Obavestenja>();
            Racuns = new HashSet<Racun>();
            Termins = new HashSet<Termin>();
            Ucesnikligas = new HashSet<UcesnikLiga>();
            Ucesnikturnirs = new HashSet<UcesnikTurnir>();
            Uplata = new HashSet<Uplata>();
            Zaduzenja = new HashSet<Zaduzenje>();
        }

        public int Id { get; set; }
        public int KlubId { get; set; }
        public string FullName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int Type { get; set; }
        public int Pol { get; set; }
        public int Year { get; set; }
        public string Contact { get; set; } = null!;
        public string? Email { get; set; }
        public string Jmbg { get; set; } = null!;
        public int FreeTermin { get; set; }
        public string? ProfileImageUrl { get; set; }

        [JsonIgnore]
        public virtual Klub Klub { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Obavestenja> Obavestenjas { get; set; }
        [JsonIgnore]
        public virtual ICollection<Racun> Racuns { get; set; }
        [JsonIgnore]
        public virtual ICollection<Termin> Termins { get; set; }
        [JsonIgnore]
        public virtual ICollection<UcesnikLiga> Ucesnikligas { get; set; }
        [JsonIgnore]
        public virtual ICollection<UcesnikTurnir> Ucesnikturnirs { get; set; }
        [JsonIgnore]
        public virtual ICollection<Uplata> Uplata { get; set; }
        [JsonIgnore]
        public virtual ICollection<Zaduzenje> Zaduzenja { get; set; }
    }
    
}
