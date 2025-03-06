using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator.SwissQrCode;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using UniversalEsir.Enums.AppMain.Statistic.SportSchedulerEnumerations;
using UniversalEsir_SportSchedulerAPI.ResponseModel.User;
using UniversalEsir_SportSchedulerAPI.ResponseModel.Tereni;
using DocumentFormat.OpenXml.Wordprocessing;

namespace UniversalEsir.Models.AppMain.Statistic.Tereni
{
    public class Teren : ObservableObject
    {
        private int _id;
        private int _klubId;
        private string _name;

        public Teren() { }
        public Teren(TerenResponse teren)
        {
            Id = teren.Id;
            KlubId = teren.KlubId;
            Name = teren.Name;
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
