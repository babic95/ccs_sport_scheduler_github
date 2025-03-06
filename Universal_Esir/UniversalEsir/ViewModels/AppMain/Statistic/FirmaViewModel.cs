using UniversalEsir.Commands.AppMain.Admin;
using UniversalEsir.Commands.AppMain.Statistic.Firma;
using UniversalEsir.Commands.AppMain.Statistic.KEP;
using UniversalEsir.Models.AppMain.Statistic;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.ViewModels.AppMain.Statistic
{
    public class FirmaViewModel : ViewModelBase
    {
        #region Fields
        private Firma _firma;
        #endregion Fields

        #region Constructors
        public FirmaViewModel()
        {
            SqliteDbContext sqliteDbContext = new SqliteDbContext();

            FirmaDB = sqliteDbContext.Firmas.FirstOrDefault();

            if(FirmaDB != null)
            {
                Firma = new Firma(FirmaDB);
            }
            else
            {
                Firma = new Firma();
            }
        }
        #endregion Constructors

        #region Properties internal
        internal FirmaDB? FirmaDB { get; set; }
        #endregion Properties internal

        #region Properties
        public Firma Firma
        {
            get { return _firma; }
            set
            {
                _firma = value;
                OnPropertyChange(nameof(Firma));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand SaveFirmaCommand => new SaveFirmaCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
