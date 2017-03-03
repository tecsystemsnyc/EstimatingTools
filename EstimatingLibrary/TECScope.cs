﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{

    public abstract class TECScope : TECObject
    {
        #region Properties

        protected string _name;
        protected string _description;
        protected Guid _guid;
        protected int _quantity;
        protected TECLocation _location;

        protected ObservableCollection<TECTag> _tags;
        protected ObservableCollection<TECAssociatedCost> _associatedCosts;

        public string Name {
            get { return _name; }
            set
            {
                var temp = this.Copy();
                _name = value;
                // Call RaisePropertyChanged whenever the property is updated
                NotifyPropertyChanged("Name", temp, this);
            }
        }
        public string Description {
            get { return _description; }
            set
            {
                var temp = this.Copy();
                _description = value;
                // Call RaisePropertyChanged whenever the property is updated
                NotifyPropertyChanged("Description", temp, this);
            
            }
        }
        public Guid Guid
        {
            get { return _guid; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                var temp = this.Copy();
                _quantity = value;
                NotifyPropertyChanged("Quantity", temp, this);
                
            }
        }
        public ObservableCollection<TECTag> Tags
        {
            get { return _tags; }
            set
            {
                var temp = this.Copy();
                _tags = value;
                NotifyPropertyChanged("Tags", temp, this);
                Tags.CollectionChanged += collectionChanged;
            }
        }
        public ObservableCollection<TECAssociatedCost> AssociatedCosts
        {
            get { return _associatedCosts; }
            set
            {
                var temp = this.Copy();
                _associatedCosts = value;
                NotifyPropertyChanged("AssociatedCosts", temp, this);
                AssociatedCosts.CollectionChanged += collectionChanged;
            }
        }

        public TECLocation Location
        {
            get { return _location; }
            set
            {
                var oldNew = Tuple.Create<Object, Object>(_location, value);
                var temp = Copy();
                _location = value;
                NotifyPropertyChanged("Location", temp, this);
                temp = Copy();
                NotifyPropertyChanged("LocationChanged", temp, oldNew);
            }
        }
        #endregion //Properties

        #region Constructors
        public TECScope(Guid guid)
        {
            _name = "";
            _description = "";
            _guid = guid;

            _quantity = 1;
            _tags = new ObservableCollection<TECTag>();
            _associatedCosts = new ObservableCollection<TECAssociatedCost>();
            Tags.CollectionChanged += collectionChanged;
            AssociatedCosts.CollectionChanged += collectionChanged;
        }

        abstract public Object DragDropCopy();
        #endregion //Constructors

        #region Methods

        private void collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
        }

        #endregion Methods

    }
}
