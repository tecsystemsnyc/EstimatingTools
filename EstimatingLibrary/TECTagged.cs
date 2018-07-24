using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public abstract class TECTagged : TECObject, IRelatable
    {
        #region Properties

        protected string _name;
        protected string _description;
        
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != Name)
                {
                    var old = Name;
                    _name = value;
                    notifyCombinedChanged(Change.Edit, "Name", this, value, old);
                }
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                if (value != Description)
                {
                    var old = Description;
                    _description = value;
                    notifyCombinedChanged(Change.Edit, "Description", this, value, old);
                }
            }
        }

        public ObservableCollection<TECTag> Tags { get; } = new ObservableCollection<TECTag>();
        
        public SaveableMap PropertyObjects
        {
            get
            {
                return propertyObjects();
            }
        }
        public SaveableMap LinkedObjects
        {
            get
            {
                SaveableMap map = linkedObjects();
                return map;
            }
        }
        #endregion

        #region Constructors
        public TECTagged(Guid guid) : base(guid)
        {
            _name = "";
            _description = "";
            _guid = guid;

            Tags.CollectionChanged += (sender, args) => taggedCollectionChanged(sender, args, "Tags");
        }

        #endregion 

        #region Methods
        protected void copyPropertiesFromTagged(TECTagged tagged)
        {
            _name = tagged.Name;
            _description = tagged.Description;
            Tags.ObservablyClear();
            foreach (TECTag tag in tagged.Tags)
            { Tags.Add(tag); }
        }
        protected virtual void taggedCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        //Is virtual so that it can be overridden in TECTypical
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, notifyCombinedChanged);
        }
        
        protected virtual SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(this.Tags, "Tags");
            return saveList;
        }
        protected virtual SaveableMap linkedObjects()
        {
            SaveableMap relatedList = new SaveableMap();
            relatedList.AddRange(this.Tags, "Tags");
            return relatedList;
        }
        #endregion Methods
    }
}
