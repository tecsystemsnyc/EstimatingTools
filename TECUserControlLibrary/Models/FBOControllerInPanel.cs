using EstimatingLibrary;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;

namespace TECUserControlLibrary.Models
{
    public class FBOControllerInPanel : ControllerInPanel
    {
        private TECFBOController _controller;
        public new TECFBOController Controller
        {
            get { return _controller; }
            set
            {
                _controller = value;
                raisePropertyChanged("Controller");
            }
        }

        private String _pointlabel = "";
        private int _pointQuantity = 1;
        private IOType _selectedPointType = IOType.AI;

        public String PointLabel
        {
            get { return _pointlabel; }
            set
            {
                _pointlabel = value;
                raisePropertyChanged("PointLabel");
            }
        }
        public int PointQuantity
        {
            get { return _pointQuantity; }
            set
            {
                _pointQuantity = value;
                raisePropertyChanged("PointQuantity");
            }
        }
        public IOType SelectedPointType
        {
            get { return _selectedPointType; }
            set
            {
                _selectedPointType = value;
                raisePropertyChanged("SelectedPointType");
            }
        }
        public List<IOType> PointTypes { get { return TECIO.PointIO; } }
        public RelayCommand AddPointCommand { get; private set; }
        public RelayCommand<TECPoint> DeletePointCommand { get; private set; }

        public FBOControllerInPanel(TECFBOController controller, TECPanel panel) : base(controller, panel)
        {
            _controller = controller;
            AddPointCommand = new RelayCommand(addPointExecute, canAddPoint);
            DeletePointCommand = new RelayCommand<TECPoint>(deletePointExecute, canDeletePoint);
        }

        private void deletePointExecute(TECPoint obj)
        {
            (Controller as TECFBOController).Points.Remove(obj);
        }

        private bool canDeletePoint(TECPoint arg)
        {
            return Controller is TECFBOController fbo && fbo.Points.Contains(arg);
        }

        private void addPointExecute()
        {
            TECPoint newPoint = new TECPoint();
            newPoint.Type = SelectedPointType;
            newPoint.Quantity = PointQuantity;
            newPoint.Label = PointLabel;

            (Controller as TECFBOController)?.Points.Add(newPoint);

            PointQuantity = 1;
            PointLabel = "";
            SelectedPointType = IOType.AI;
        }

        private bool canAddPoint()
        {
            return PointQuantity != 0 && Controller is TECFBOController;
        }

    }
}