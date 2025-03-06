using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalEsir.Models.AppMain.Statistic.Driver
{
    public class Driver : ObservableObject
    {
        private int _id;
        private string _name;
        private string _jmbg;
        private string _displayName;
        private string _city;
        private string _address;
        private string _contractNumber;
        private string _email;
        private bool _hasDelivery;
        private string _isporuka;
        private string _colorSet;

        public Driver() { }
        public Driver(DriverDB driver, 
            bool hasDelivery)
        {
            Id = driver.Id;
            Name = driver.Name;
            Jmbg = driver.Jmbg;
            DisplayName = $"{driver.Id} - {driver.Name}";
            City = driver.City;
            Address = driver.Address;
            ContractNumber = driver.ContractNumber;
            Email = driver.Email;
            HasDelivery = hasDelivery;

            if (hasDelivery)
            {
                Isporuka = "DA";
                ColorSet = "Yellow";
            }
            else
            {
                Isporuka = "NE";
                ColorSet = "Transparent";
            }
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
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChange(nameof(Name));
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
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChange(nameof(DisplayName));
            }
        }
        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                OnPropertyChange(nameof(City));
            }
        }
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChange(nameof(Address));
            }
        }
        public string ContractNumber
        {
            get { return _contractNumber; }
            set
            {
                _contractNumber = value;
                OnPropertyChange(nameof(ContractNumber));
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
        public bool HasDelivery
        {
            get { return _hasDelivery; }
            set
            {
                _hasDelivery = value;
                OnPropertyChange(nameof(HasDelivery));
            }
        }
        public string Isporuka
        {
            get { return _isporuka; }
            set
            {
                _isporuka = value;
                OnPropertyChange(nameof(Isporuka));
            }
        }
        public string ColorSet
        {
            get { return _colorSet; }
            set
            {
                _colorSet = value;
                OnPropertyChange(nameof(ColorSet));
            }
        }
    }
}
