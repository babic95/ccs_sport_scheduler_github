using UniversalEsir.Commands.AppMain.Statistic.Norm;
using DocumentFormat.OpenXml.VariantTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalEsir.ViewModels.AppMain.Statistic
{
    public class NormViewModel : ViewModelBase
    {
        #region Fields
        private DateTime? _fromDate;
        private DateTime? _toDate;
        #endregion Fields

        #region Constructors
        public NormViewModel()
        {
            FromDate = null; 
            ToDate = null;
        }
        #endregion Constructors

        #region Properties internal
        #endregion Properties internal

        #region Properties
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                if (value != null &&
                    value.HasValue)
                {
                    _fromDate = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 5, 0, 0);
                }
                else
                {
                    _fromDate = value;
                }
                OnPropertyChange(nameof(FromDate));
            }
        }
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                if (value != null &&
                    value.HasValue)
                {
                    value = value.Value.AddDays(1);
                    _toDate = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 4, 59, 59);
                }
                else
                {
                    _toDate = value;
                }
                OnPropertyChange(nameof(ToDate));
            }
        }
        #endregion Properties

        #region Commands
        public ICommand FixNormCommand => new FixNormCommand(this);
        public ICommand PrintAllNormCommand => new PrintAllNormCommand(this);
        #endregion Commands

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}