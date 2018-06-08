using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EstimatingLibrary.Utilities
{
    public class ChangeWatcher
    {
        public readonly List<string> propertyExceptions = new List<string>
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
        #endregion

    }
}