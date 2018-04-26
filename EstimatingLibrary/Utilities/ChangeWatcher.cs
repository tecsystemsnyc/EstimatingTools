using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EstimatingLibrary.Utilities
{
    public class ChangeWatcher
    {
        private readonly List<string> propertyExceptions = new List<string>
        {
            "TypicalInstanceDictionary",
            "TemplateRelationship"
        };

        #region Constructors
        public ChangeWatcher(ITECObject item)
        {
            register(item);
        }
        #endregion

        #region Events
        /// <summary>
        /// Adds, removes, edit raised from models
        /// </summary>
        public event Action<TECChangedEventArgs> Changed;
        /// <summary>
        /// All Changed events where the object is not typical
        /// </summary>
        public event Action<TECChangedEventArgs> InstanceChanged;
        /// <summary>
        /// Changes in cost raised from models
        /// </summary>
        public event Action<CostBatch> CostChanged;
        /// <summary>
        /// Changes in point raised from models
        /// </summary>
        public event Action<int> PointChanged;
        /// <summary>
        /// INotifyPropertyChanged events from all registered objects
        /// </summary>
        public event Action<object, PropertyChangedEventArgs> PropertyChanged;
        /// <summary>
        /// Add, Remove events from all instances and their PropertyObjects
        /// </summary>
        public event Action<Change, ITECObject> InstanceConstituentChanged;
        /// <summary>
        /// Add, Remove events from all typical objects and their PropertyObjects
        /// </summary>
        public event Action<Change, ITECObject> TypicalConstituentChanged;
        #endregion

        #region Methods
        public void Refresh(ITECObject item)
        {
            register(item);
        }
        
        private void register(ITECObject item)
        {
            registerTECObject(item);
            if (item is IRelatable saveable)
            {
                saveable.GetDirectChildren().ForEach(register);
            }
        }
        private void registerTECObject(ITECObject ob)
        {
            ob.TECChanged += handleTECChanged;
            ob.PropertyChanged += raisePropertyChanged;
            if (ob is INotifyCostChanged costOb)
            {
                costOb.CostChanged += (e) => raiseCostChanged(ob, e);
            }
            if (ob is INotifyPointChanged pointOb)
            {
                pointOb.PointChanged += (e) => raisePointChanged(ob, e);
            }
        }
        private void registerChange(TECChangedEventArgs args)
        {
            if(!propertyExceptions.Contains(args.PropertyName))
            {
                if (args.Change == Change.Add && args.Value is ITECObject tObj)
                {
                    register(tObj);
                }
                else if (args.Change == Change.Edit && args.Sender is IRelatable saveable)
                {
                    if (!saveable.LinkedObjects.Contains(args.PropertyName) && args.Value is ITECObject tValue)
                    {
                        register(tValue);
                    }
                }
            }
        }
        private void handleTECChanged(TECChangedEventArgs e)
        {
            registerChange(e);
            raiseChanged(e);

            if (!propertyExceptions.Contains(e.PropertyName) && !(e.Sender is TECCatalogs))
            {
                if (e.Value is ITypicalable valueTyp)
                {
                    raiseIfTypical(valueTyp);
                }
                else
                {
                    if (e.Sender is ITypicalable senderTyp)
                    {
                        raiseIfTypical(senderTyp);
                    }
                    else
                    {
                        raiseInstanceChanged(e);
                    }
                }
            }

            void raiseIfTypical(ITypicalable item)
            {
                if (!item.IsTypical)
                {
                    raiseInstanceChanged(e);
                }
                else
                {
                    raiseTypicalConsituentChanged(e);
                }
            }
        }

        private void raiseTypicalConsituentChanged(TECChangedEventArgs e)
        {
            if ((e.Change == Change.Add || e.Change == Change.Remove) && e.Sender is IRelatable parent)
            {
                if (!parent.LinkedObjects.Contains(e.PropertyName))
                    raiseTypicalConstituents(e.Change, e.Value as ITECObject);
            }
        }

        private void raisePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
        private void raiseChanged(TECChangedEventArgs e)
        {
            Changed?.Invoke(e);
        }
        private void raiseCostChanged(ITECObject sender, CostBatch obj)
        {
            CostChanged?.Invoke(obj);
        }
        private void raisePointChanged(ITECObject sender, int num)
        {
            PointChanged?.Invoke(num);
        }
        private void raiseInstanceChanged(TECChangedEventArgs e)
        {
            InstanceChanged?.Invoke(e);
            if((e.Change == Change.Add || e.Change == Change.Remove) && e.Sender is IRelatable parent)
            {
                if(!parent.LinkedObjects.Contains(e.PropertyName))
                    raiseConstituents(e.Change, e.Value as ITECObject);
            }
        }
        private void raiseConstituents(Change change, ITECObject item)
        {
            InstanceConstituentChanged?.Invoke(change, item);
            if(item is IRelatable parent)
            {
                foreach(var child in parent.GetDirectChildren())
                {
                    raiseConstituents(change, child);
                }
            }
        }
        private void raiseTypicalConstituents(Change change, ITECObject item)
        {
            TypicalConstituentChanged?.Invoke(change, item);
            if (item is IRelatable parent)
            {
                parent.GetDirectChildren().Where(x => x is ITypicalable typ && typ.IsTypical).
                    ForEach(child => raiseTypicalConstituents(change, child));
            }
        }
        
        #endregion

    }
}