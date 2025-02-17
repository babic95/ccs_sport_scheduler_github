using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir_Database.Models;
using UniversalEsir_SportSchedulerAPI.ResponseModel.User;

namespace UniversalEsir.Models.AppMain.Statistic.Clanovi
{
    public class Clan : ObservableObject
    {
        private int _id;
        private int _klubId;
        private string _fullName;
        private string _password;
        private string _username;
        private ClanEnumeration _type;
        private DateTime _birthday;
        private string _contact;
        private string _email;
        private string _jmbg;
        private int _freeTermin;

        public Clan() { }
        public Clan(UserResponse clan)
        {
            Id = clan.Id;
            KlubId = clan.KlubId;
            FullName = clan.FullName;
            Password = clan.Password;
            Username = clan.Username;
            Type = (ClanEnumeration)clan.Type;
            Birthday = clan.Birthday;
            Contact = clan.Contact;
            Email = clan.Email;
            Jmbg = clan.Jmbg;
            FreeTermin = clan.FreeTermin;
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public int KlubId
        {
            get { return _klubId; }
            set
            {
                _klubId = value;
                OnPropertyChange(nameof(KlubId));
            }
        }
        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                OnPropertyChange(nameof(FullName));
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChange(nameof(Password));
            }
        }
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChange(nameof(Username));
            }
        }
        public ClanEnumeration Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChange(nameof(Type));
            }
        }
        public DateTime Birthday
        {
            get { return _birthday; }
            set
            {
                _birthday = value;
                OnPropertyChange(nameof(Birthday));
            }
        }
        public string Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
                OnPropertyChange(nameof(Contact));
            }
        }
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChange(nameof(Email));
            }
        }
        public string Jmbg
        {
            get { return _jmbg; }
            set
            {
                _jmbg = value;
                OnPropertyChange(nameof(Jmbg));
            }
        }
        public int FreeTermin
        {
            get { return _freeTermin; }
            set
            {
                _freeTermin = value;
                OnPropertyChange(nameof(FreeTermin));
            }
        }
    }
}
