using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LaborVM : ViewModelBase
    {
        public TECBid Bid { get; }
        public TECScopeTemplates Templates { get { return Bid.Templates; } }
        public TECEstimator Estimate { get; }
        public ICommand ReloadCommand { get; private set; }
        public RelayCommand<TECParameters> SetParametersCommand { get; private set; }
        public RelayCommand SetDesiredConfidenceCommand { get; private set; }

        private Confidence _desiredConfidence;
        public Confidence DesiredConfidence
        {
            get { return _desiredConfidence; }
            set
            {
                _desiredConfidence = value;
                RaisePropertyChanged("DesiredConfidence");
            }
        }
        
        public LaborVM(TECBid bid, TECEstimator estimate)
        {
            Bid = bid;
            Estimate = estimate;
            DesiredConfidence = bid.Parameters.DesiredConfidence;

            SetParametersCommand = new RelayCommand<TECParameters>(SetParametersExecute);
            SetDesiredConfidenceCommand = new RelayCommand(SetConfidenceExecute, CanSetConfidence);
        }

        private void SetConfidenceExecute()
        {
            Bid.Parameters.DesiredConfidence = DesiredConfidence;
        }

        private bool CanSetConfidence()
        {
            return DesiredConfidence != Bid.Parameters.DesiredConfidence;
        }
        
        private void SetParametersExecute(TECParameters obj)
        {
            Bid.Parameters = obj;
        }
    }
}