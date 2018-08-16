using EstimatingLibrary.Interfaces;
using System;
using System.Collections;
using System.ComponentModel;

namespace EstimatingLibrary
{
    public enum Change { Add, Remove, Edit }

    public abstract class TECObject : INotifyPropertyChanged, INotifyTECChanged, ITECObject, IDoRedoable
    {
        #region Properties
        protected Guid _guid;
        
        public Guid Guid
        {
            get { return _guid; }
        }

        #endregion

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<TECChangedEventArgs> TECChanged;

        protected void notifyTECChanged(Change change, string propertyName, ITECObject sender,
            object value, object oldValue = null)
        {
            TECChangedEventArgs args = new TECChangedEventArgs(change, propertyName, sender, value, oldValue);
            TECChanged?.Invoke(args);
        }

        protected void notifyCombinedChanged(Change change, string propertyName, ITECObject sender,
            object value, object oldValue = null)
        {
            TECChangedEventArgs args = new TECChangedEventArgs(change, propertyName, sender, value, oldValue);
            PropertyChanged?.Invoke(sender, args);
            TECChanged?.Invoke(args);
        }

        protected void raisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion //Property Changed

        public TECObject(Guid guid)
        {
            _guid = guid;
        }

        #region IDoRedoable

        public virtual bool CanDo => true;

        public virtual void AddForProperty(string propertyName, object item)
        {
            var property = this.GetType().GetProperty(propertyName);
            var parentCollection = property.GetValue(this);
            ((IList)parentCollection).Add(item);
        }

        public virtual void RemoveForProperty(string propertyName, object item)
        {
            var property = this.GetType().GetProperty(propertyName);
            var parentCollection = property.GetValue(this);
            ((IList)parentCollection).Remove(item);
        }

        public virtual void SetProperty(string propertyName, object value)
        {
            var property = this.GetType().GetProperty(propertyName);
            property.SetValue(this, value);
        }

        #endregion
    }
}
