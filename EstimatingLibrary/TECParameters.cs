﻿using System;
using System.Collections.Generic;

namespace EstimatingLibrary
{
    public class TECParameters : TECLabeled
    {
        #region Properties
        private double _escalation;
        private double _subcontractorEscalation;
        private double _warranty;
        private double _shipping;
        private double _tax;
        private double _subcontractorWarranty;
        private double _subcontractorShipping;
        private double _bondRate;
        private double _markup;

        private bool _isTaxExempt;
        private bool _requiresBond;
        private bool _requiresWrapUp;
        private bool _hasBMS;

        public double Escalation
        {
            get { return _escalation; }
            set
            {
                var old = Escalation;
                _escalation = value;
                notifyCombinedChanged(Change.Edit, "Escalation", this, value, old);
            }
        }
        public double SubcontractorEscalation
        {
            get { return _subcontractorEscalation; }
            set
            {
                var old = SubcontractorEscalation;
                _subcontractorEscalation = value;
                notifyCombinedChanged(Change.Edit, "SubcontractorEscalation", this, value, old);
            }
        }
        public double Warranty
        {
            get { return _warranty; }
            set
            {
                var old = Warranty;
                _warranty = value;
                notifyCombinedChanged(Change.Edit, "Warranty", this, value, old);
            }
        }
        public double Shipping
        {
            get { return _shipping; }
            set
            {
                var old = Shipping;
                _shipping = value;
                notifyCombinedChanged(Change.Edit, "Shipping", this, value, old);
            }
        }
        public double Tax
        {
            get { return _tax; }
            set
            {
                var old = Tax;
                _tax = value;
                notifyCombinedChanged(Change.Edit, "Tax", this, value, old);
            }
        }
        public double SubcontractorWarranty
        {
            get { return _subcontractorWarranty; }
            set
            {
                var old = SubcontractorWarranty;
                _subcontractorWarranty = value;
                notifyCombinedChanged(Change.Edit, "SubcontractorWarranty", this, value, old);
            }
        }
        public double SubcontractorShipping
        {
            get { return _subcontractorShipping; }
            set
            {
                var old = Shipping;
                _subcontractorShipping = value;
                notifyCombinedChanged(Change.Edit, "SubcontractorShipping", this, value, old);
            }
        }
        public double BondRate
        {
            get { return _bondRate; }
            set
            {
                var old = BondRate;
                _bondRate = value;
                notifyCombinedChanged(Change.Edit, "BondRate", this, value, old);
            }
        }
        public double Markup
        {
            get { return _markup; }
            set
            {
                var old = Markup;
                _markup = value;
                notifyCombinedChanged(Change.Edit, "Markup", this, value, old);
            }
        }

        public bool IsTaxExempt
        {
            get { return _isTaxExempt; }
            set
            {
                var old = IsTaxExempt;
                _isTaxExempt = value;
                notifyCombinedChanged(Change.Edit, "IsTaxExempt", this, value, old);
            }
        }
        public bool RequiresBond
        {
            get { return _requiresBond; }
            set
            {
                var old = RequiresBond;
                _requiresBond = value;
                notifyCombinedChanged(Change.Edit, "RequiresBond", this, value, old);
            }
        }
        public bool RequiresWrapUp
        {
            get { return _requiresWrapUp; }
            set
            {
                var old = RequiresWrapUp;
                _requiresWrapUp = value;
                notifyCombinedChanged(Change.Edit, "RequiresWrapUp", this, value, old);
            }
        }
        public bool HasBMS
        {
            get { return _hasBMS; }
            set
            {
                var old = HasBMS;
                _hasBMS = value;
                notifyCombinedChanged(Change.Edit, "HasBMS", this, value, old);
            }
        }
        #endregion
        #region Labor

        private Confidence _desiredConfidence;
        public Confidence DesiredConfidence
        {
            get { return _desiredConfidence; }
            set
            {
                var old = _desiredConfidence;
                _desiredConfidence = value;
                notifyCombinedChanged(Change.Edit, "DesiredConfidence", this, value, old);
                raisePropertyChanged("PMExtenedCoef");
                raisePropertyChanged("EngExtenedCoef");
                raisePropertyChanged("SoftExtenedCoef");
                raisePropertyChanged("GraphExtenedCoef");
                raisePropertyChanged("CommExtenedCoef");
            }
        }

        #region PM
        private double _pmCoef;
        public double PMCoef
        {
            get { return _pmCoef; }
            set
            {
                var old = PMCoef;
                _pmCoef = value;
                notifyCombinedChanged(Change.Edit, "PMCoef", this, value, old);
            }
        }

        private double _pmCoefStdError;
        public double PMCoefStdError
        {
            get { return _pmCoefStdError; }
            set
            {
                var old = PMCoefStdError;
                _pmCoefStdError = value;
                notifyCombinedChanged(Change.Edit, "PMCoefStdError", this, value, old);
            }
        }

        private double _pmRate;
        public double PMRate
        {
            get { return _pmRate; }
            set
            {
                var old = PMRate;
                _pmRate = value;
                notifyCombinedChanged(Change.Edit, "PMRate", this, value, old);

            }
        }

        public double PMExtenedCoef
        {
            get { return effectiveCoef(PMCoef, PMCoefStdError, DesiredConfidence); }
        }

        #endregion PM

        #region ENG
        private double _engCoef;
        public double ENGCoef
        {
            get { return _engCoef; }
            set
            {
                var old = ENGCoef;
                _engCoef = value;
                notifyCombinedChanged(Change.Edit, "ENGCoef", this, value, old);


            }
        }

        private double _engCoefStdError;
        public double ENGCoefStdError
        {
            get { return _engCoefStdError; }
            set
            {
                var old = ENGCoefStdError;
                _engCoefStdError = value;
                notifyCombinedChanged(Change.Edit, "ENGCoefStdError", this, value, old);
            }
        }

        private double _engRate;
        public double ENGRate
        {
            get { return _engRate; }
            set
            {

                var old = ENGRate;
                _engRate = value;
                notifyCombinedChanged(Change.Edit, "ENGRate", this, value, old);


            }
        }

        public double ENGExtenedCoef
        {
            get { return effectiveCoef(ENGCoef, ENGCoefStdError, DesiredConfidence); }
        }
        #endregion ENG

        #region Comm
        private double _commCoef;
        public double CommCoef
        {
            get { return _commCoef; }
            set
            {

                var old = CommCoef;
                _commCoef = value;
                notifyCombinedChanged(Change.Edit, "CommCoef", this, value, old);


            }
        }

        private double _commCoefStdError;
        public double CommCoefStdError
        {
            get { return _commCoefStdError; }
            set
            {
                var old = CommCoefStdError;
                _commCoefStdError = value;
                notifyCombinedChanged(Change.Edit, "CommCoefStdError", this, value, old);
            }
        }

        private double _commRate;
        public double CommRate
        {
            get { return _commRate; }
            set
            {

                var old = CommRate;
                _commRate = value;
                notifyCombinedChanged(Change.Edit, "CommRate", this, value, old);


            }
        }

        public double CommExtenedCoef
        {
            get { return effectiveCoef(CommCoef, CommCoefStdError, DesiredConfidence); }
        }
        #endregion Comm

        #region Soft
        private double _softCoef;
        public double SoftCoef
        {
            get { return _softCoef; }
            set
            {

                var old = SoftCoef;
                _softCoef = value;
                notifyCombinedChanged(Change.Edit, "SoftCoef", this, value, old);


            }
        }

        private double _softCoefStdError;
        public double SoftCoefStdError
        {
            get { return _softCoefStdError; }
            set
            {
                var old = SoftCoefStdError;
                _softCoefStdError = value;
                notifyCombinedChanged(Change.Edit, "SoftCoefStdError", this, value, old);
            }
        }

        private double _softRate;
        public double SoftRate
        {
            get { return _softRate; }
            set
            {
                var old = SoftRate;
                _softRate = value;
                notifyCombinedChanged(Change.Edit, "SoftRate", this, value, old);

            }
        }

        public double SoftExtenedCoef
        {
            get { return effectiveCoef(SoftCoef, SoftCoefStdError, DesiredConfidence); }
        }
        #endregion Soft

        #region Graph
        private double _graphCoef;
        public double GraphCoef
        {
            get { return _graphCoef; }
            set
            {
                var old = GraphCoef;
                _graphCoef = value;
                notifyCombinedChanged(Change.Edit, "GraphCoef", this, value, old);

            }
        }

        private double _graphCoefStdError;
        public double GraphCoefStdError
        {
            get { return _graphCoefStdError; }
            set
            {
                var old = GraphCoefStdError;
                _graphCoefStdError = value;
                notifyCombinedChanged(Change.Edit, "GraphCoefStdError", this, value, old);
            }
        }

        private double _graphRate;
        public double GraphRate
        {
            get { return _graphRate; }
            set
            {
                var old = GraphRate;
                _graphRate = value;
                notifyCombinedChanged(Change.Edit, "GraphRate", this, value, old);

            }
        }

        public double GraphExtenedCoef
        {
            get { return effectiveCoef(GraphCoef, GraphCoefStdError, DesiredConfidence); }
        }
        #endregion Graph

        #region Electrical
        private double _electricalRate;
        public double ElectricalRate
        {
            get { return _electricalRate; }
            set
            {
                var old = ElectricalRate;
                _electricalRate = value;
                notifyCombinedChanged(Change.Edit, "ElectricalRate", this, value, old);
                raisePropertyChanged("ElectricalEffectiveRate");
            }
        }

        private double _electricalNonUnionRate;
        public double ElectricalNonUnionRate
        {
            get { return _electricalNonUnionRate; }
            set
            {
                var old = ElectricalNonUnionRate;
                _electricalNonUnionRate = value;
                notifyCombinedChanged(Change.Edit, "ElectricalNonUnionRate", this, value, old);
                raisePropertyChanged("ElectricalEffectiveRate");
            }
        }

        public double ElectricalEffectiveRate
        {
            get
            {
                double rate;
                if (ElectricalIsUnion)
                {
                    rate = ElectricalRate;
                }
                else
                {
                    rate = ElectricalNonUnionRate;
                }

                if (ElectricalIsOnOvertime)
                {
                    return (rate * 1.5);
                }
                else
                {
                    return rate;
                }
            }
        }

        private double _electricalSuperRate;
        public double ElectricalSuperRate
        {
            get { return _electricalSuperRate; }
            set
            {
                var old = ElectricalSuperRate;
                _electricalSuperRate = value;
                notifyCombinedChanged(Change.Edit, "ElectricalSuperRate", this, value, old);
                raisePropertyChanged("ElectricalSuperEffectiveRate");
            }
        }

        private double _electricalSuperNonUnionRate;
        public double ElectricalSuperNonUnionRate
        {
            get { return _electricalSuperNonUnionRate; }
            set
            {
                var old = ElectricalSuperNonUnionRate;
                _electricalSuperNonUnionRate = value;
                notifyCombinedChanged(Change.Edit, "ElectricalSuperNonUnionRate", this, value, old);
                raisePropertyChanged("ElectricalSuperEffectiveRate");
            }
        }

        public double ElectricalSuperEffectiveRate
        {
            get
            {
                double rate;
                if (ElectricalIsUnion)
                {
                    rate = ElectricalSuperRate;
                }
                else
                {
                    rate = ElectricalSuperNonUnionRate;
                }

                if (ElectricalIsOnOvertime)
                {
                    return (rate * 1.5);
                }
                else
                {
                    return rate;
                }
            }
        }

        private bool _electricalIsOnOvertime;
        public bool ElectricalIsOnOvertime
        {
            get { return _electricalIsOnOvertime; }
            set
            {
                var old = ElectricalIsOnOvertime;
                _electricalIsOnOvertime = value;
                notifyCombinedChanged(Change.Edit, "ElectricalIsOnOvertime", this, value, old);
                raiseEffectiveRateChanged();
            }
        }

        private bool _electricalIsUnion;
        public bool ElectricalIsUnion
        {
            get { return _electricalIsUnion; }
            set
            {
                var old = ElectricalIsUnion;
                _electricalIsUnion = value;
                notifyCombinedChanged(Change.Edit, "ElectricalIsUnion", this, value, old);
                raiseEffectiveRateChanged();
            }
        }

        private double _electricalSuperRatio;
        public double ElectricalSuperRatio
        {
            get { return _electricalSuperRatio; }
            set
            {
                var old = ElectricalSuperRatio;
                _electricalSuperRatio = value;
                notifyCombinedChanged(Change.Edit, "ElectricalSuperRatio", this, value, old);
            }
        }

        #endregion Electrical

        #endregion
        public TECParameters(Guid guid) : base(guid)
        {
            _isTaxExempt = false;
            _requiresBond = false;
            _requiresWrapUp = false;
            _hasBMS = true;

            _desiredConfidence = Confidence.NinetyFive;

            _escalation = 0;
            _subcontractorEscalation = 0;
            _warranty = 5.0;
            _shipping = 3.0;
            _tax = 8.75;

            _subcontractorShipping = 3.0;
            _subcontractorWarranty = 5.0;

            _pmCoef = 1.0;
            _pmCoefStdError = 1.0;
            _pmRate = 0;

            _engCoef = 1.0;
            _engCoefStdError = 1.0;
            _engRate = 0;

            _commCoef = 1.0;
            _commCoefStdError = 1.0;
            _commRate = 0;

            _softCoef = 1.0;
            _softCoefStdError = 1.0;
            _softRate = 0;

            _graphCoef = 1.0;
            _graphCoefStdError = 1.0;
            _graphRate = 0;

            _electricalRate = 0;
            _electricalNonUnionRate = 0;
            _electricalSuperRate = 0;
            _electricalSuperNonUnionRate = 0;

            _electricalIsOnOvertime = false;
            _electricalIsUnion = true;
        }

        public TECParameters(TECParameters parametersSource) : this(parametersSource.Guid)
        {
            _isTaxExempt = parametersSource.IsTaxExempt;
            _requiresBond = parametersSource.RequiresBond;
            _requiresWrapUp = parametersSource.RequiresWrapUp;
            _hasBMS = parametersSource.HasBMS;

            _escalation = parametersSource.Escalation;
            _subcontractorEscalation = parametersSource.SubcontractorEscalation;

            _desiredConfidence = parametersSource.DesiredConfidence;

            _pmCoef = parametersSource.PMCoef;
            _pmCoefStdError = parametersSource.PMCoefStdError;
            _pmRate = parametersSource.PMRate;

            _engCoef = parametersSource.ENGCoef;
            _engCoefStdError = parametersSource.ENGCoefStdError;
            _engRate = parametersSource.ENGRate;

            _commCoef = parametersSource.CommCoef;
            _commCoefStdError = parametersSource.CommCoefStdError;
            _commRate = parametersSource.CommRate;

            _softCoef = parametersSource.SoftCoef;
            _softCoefStdError = parametersSource.SoftCoefStdError;
            _softRate = parametersSource.SoftRate;

            _graphCoef = parametersSource.GraphCoef;
            _graphCoefStdError = parametersSource.CommCoefStdError;
            _graphRate = parametersSource.GraphRate;

            _electricalRate = parametersSource.ElectricalRate;
            _electricalNonUnionRate = parametersSource.ElectricalNonUnionRate;
            _electricalSuperRate = parametersSource.ElectricalSuperRate;
            _electricalSuperNonUnionRate = parametersSource.ElectricalSuperNonUnionRate;
            _electricalSuperRatio = parametersSource.ElectricalSuperRatio;

            _electricalIsOnOvertime = parametersSource.ElectricalIsOnOvertime;
            _electricalIsUnion = parametersSource.ElectricalIsUnion;
        }

        public void UpdateConstants(TECParameters labor)
        {
            PMCoef = labor.PMCoef;
            PMCoefStdError = labor.PMCoefStdError;
            PMRate = labor.PMRate;

            ENGCoef = labor.ENGCoef;
            ENGCoefStdError = labor.ENGCoefStdError;
            ENGRate = labor.ENGRate;

            SoftCoef = labor.SoftCoef;
            SoftCoefStdError = labor.SoftCoefStdError;
            SoftRate = labor.SoftRate;

            GraphCoef = labor.GraphCoef;
            GraphCoefStdError = labor.GraphCoefStdError;
            GraphRate = labor.GraphRate;

            CommCoef = labor.CommCoef;
            CommCoefStdError = labor.CommCoefStdError;
            CommRate = labor.CommRate;

            ElectricalRate = labor.ElectricalRate;
            ElectricalNonUnionRate = labor.ElectricalNonUnionRate;
            ElectricalSuperRate = labor.ElectricalSuperRate;
            ElectricalSuperNonUnionRate = labor.ElectricalSuperNonUnionRate;

            ElectricalSuperRatio = labor.ElectricalSuperRatio;
        }

        private void raiseEffectiveRateChanged()
        {
            raisePropertyChanged("ElectricalEffectiveRate");
            raisePropertyChanged("ElectricalSuperEffectiveRate");
        }

        private double effectiveCoef(double coefficient, double stdError, Confidence confidence)
        {
            return coefficient + (stdError * zValues[confidence]);
        }

        private static Dictionary<Confidence, double> zValues = new Dictionary<EstimatingLibrary.Confidence, double>()
        {
            { Confidence.ThirtyThree, -0.44},
            { Confidence.Fifty, 0.0 },
            { Confidence.SixtySix, 0.43 },
            { Confidence.Eighty, 0.85 },
            { Confidence.Ninety, 1.29 },
            { Confidence.NinetyFive, 1.65 }
        };
    }

    public enum Confidence
    {
        ThirtyThree,
        Fifty,
        SixtySix,
        Eighty,
        Ninety,
        NinetyFive
    }
}
