using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Uplate;

namespace UniversalEsir.Models.AppMain.Statistic.Clanovi
{
    public class SatiTermina : ObservableObject
    {
        private int _sat;
        private string _name;

        public SatiTermina(int sat)
        {
            Sat = sat;
            Name = $"{Sat}:00";
        }

        public int Sat
        {
            get { return _sat; }
            set
            {
                _sat = value;
                OnPropertyChange(nameof(Sat));
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
    }
}