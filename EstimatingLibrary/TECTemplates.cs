﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECTemplates : TECScopeManager
    {
        #region Properties
        private ObservableCollection<TECSystem> _systemTemplates;
        private ObservableCollection<TECEquipment> _equipmentTemplates;
        private ObservableCollection<TECSubScope> _subScopeTemplates;
        private ObservableCollection<TECController> _controllerTemplates;
        private ObservableCollection<TECMisc> _miscCostTemplates;
        private ObservableCollection<TECPanel> _panelTemplates;

        public ObservableCollection<TECSystem> SystemTemplates
        {
            get { return _systemTemplates; }
            set
            {
                var temp = this.Copy();
                SystemTemplates.CollectionChanged -= CollectionChanged;
                _systemTemplates = value;
                SystemTemplates.CollectionChanged += CollectionChanged;
                NotifyPropertyChanged("SystemTemplates", temp, this);
            }
        }
        public ObservableCollection<TECEquipment> EquipmentTemplates
        {
            get { return _equipmentTemplates; }
            set
            {
                var temp = this.Copy();
                EquipmentTemplates.CollectionChanged -= CollectionChanged;
                _equipmentTemplates = value;
                EquipmentTemplates.CollectionChanged += CollectionChanged;
                NotifyPropertyChanged("EquipmentTemplates", temp, this);
            }
        }
        public ObservableCollection<TECSubScope> SubScopeTemplates
        {
            get { return _subScopeTemplates; }
            set
            {
                var temp = this.Copy();
                SubScopeTemplates.CollectionChanged -= CollectionChanged;
                _subScopeTemplates = value;
                SubScopeTemplates.CollectionChanged += CollectionChanged;
                NotifyPropertyChanged("SubScopeTemplates", temp, this);
            }
        }
        public ObservableCollection<TECController> ControllerTemplates
        {
            get { return _controllerTemplates; }
            set
            {
                var temp = this.Copy();
                ControllerTemplates.CollectionChanged -= CollectionChanged;
                _controllerTemplates = value;
                ControllerTemplates.CollectionChanged += CollectionChanged;
                NotifyPropertyChanged("ControllerTemplates", temp, this);
            }
        }
        public ObservableCollection<TECMisc> MiscCostTemplates
        {
            get { return _miscCostTemplates; }
            set
            {
                var temp = this.Copy();
                MiscCostTemplates.CollectionChanged -= CollectionChanged;
                _miscCostTemplates = value;
                MiscCostTemplates.CollectionChanged += CollectionChanged;
                NotifyPropertyChanged("MiscCostTemplates", temp, this);
            }
        }
        public ObservableCollection<TECPanel> PanelTemplates
        {
            get { return _panelTemplates; }
            set
            {
                var temp = this.Copy();
                PanelTemplates.CollectionChanged -= CollectionChanged;
                _panelTemplates = value;
                PanelTemplates.CollectionChanged += CollectionChanged;
                NotifyPropertyChanged("PanelTemplates", temp, this);
            }
        }
        #endregion //Properties

        #region Constructors
        public TECTemplates() : this(Guid.NewGuid()) { }
        public TECTemplates(Guid guid) : base(guid)
        {
            _labor = new TECLabor();

            _systemTemplates = new ObservableCollection<TECSystem>();
            _equipmentTemplates = new ObservableCollection<TECEquipment>();
            _subScopeTemplates = new ObservableCollection<TECSubScope>();
            _controllerTemplates = new ObservableCollection<TECController>();
            _miscCostTemplates = new ObservableCollection<TECMisc>();
            _panelTemplates = new ObservableCollection<TECPanel>();

            SystemTemplates.CollectionChanged += CollectionChanged;
            EquipmentTemplates.CollectionChanged += CollectionChanged;
            SubScopeTemplates.CollectionChanged += CollectionChanged;
            ControllerTemplates.CollectionChanged += CollectionChanged;
            MiscCostTemplates.CollectionChanged += CollectionChanged;
            PanelTemplates.CollectionChanged += CollectionChanged;
        }
        public TECTemplates(TECTemplates templatesSource) : this(templatesSource.Guid)
        {
            if (_labor != null)
            {
                _labor = templatesSource.Labor;
            }
            foreach (TECSystem system in templatesSource.SystemTemplates)
            { SystemTemplates.Add(new TECSystem(system)); }
            foreach (TECEquipment equip in templatesSource.EquipmentTemplates)
            { EquipmentTemplates.Add(new TECEquipment(equip)); }
            foreach (TECSubScope subScope in templatesSource.SubScopeTemplates)
            { SubScopeTemplates.Add(new TECSubScope(subScope)); }
            foreach (TECController controller in templatesSource.ControllerTemplates)
            { ControllerTemplates.Add(new TECController(controller)); }
            foreach (TECMisc cost in templatesSource.MiscCostTemplates)
            {
                MiscCostTemplates.Add(new TECMisc(cost));
            }
            foreach (TECPanel panel in templatesSource.PanelTemplates)
            {
                PanelTemplates.Add(new TECPanel(panel));
            }
        }
        #endregion //Constructors

        #region Collection Changed
        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    NotifyPropertyChanged("Add", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    NotifyPropertyChanged("Remove", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                NotifyPropertyChanged("Edit", this, sender);
            }
        }
        #endregion

        public override object Copy()
        {
            var outTemplate = new TECTemplates(Guid);
            outTemplate._catalogs = this.Catalogs.Copy() as TECCatalogs;
            if (_labor != null)
            {
                outTemplate._labor = this.Labor;
            }
            foreach (TECSystem system in this.SystemTemplates)
            { outTemplate.SystemTemplates.Add(system.Copy() as TECSystem); }
            foreach (TECEquipment equip in this.EquipmentTemplates)
            { outTemplate.EquipmentTemplates.Add(equip.Copy() as TECEquipment); }
            foreach (TECSubScope subScope in this.SubScopeTemplates)
            { outTemplate.SubScopeTemplates.Add(subScope.Copy() as TECSubScope); }
            foreach (TECController controller in this.ControllerTemplates)
            { outTemplate.ControllerTemplates.Add(controller.Copy() as TECController); }
            foreach (TECMisc cost in this.MiscCostTemplates)
            { outTemplate.MiscCostTemplates.Add(cost.Copy() as TECMisc); }
            foreach (TECPanel panel in this.PanelTemplates)
            { outTemplate.PanelTemplates.Add(panel.Copy() as TECPanel); }
            return outTemplate;
        }

    }
}
