using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities.WatcherFilters
{
    public abstract class BaseWatcherFilter
    {
        protected readonly List<string> propertyExceptions;

        public BaseWatcherFilter(ChangeWatcher watcher)
        {
            watcher.Changed += watcherChanged;
            watcher.CostChanged += watcherCostChanged;
            watcher.PointChanged += watcherPointChanged;
            watcher.PropertyChanged += watcherPropertyChanged;

            this.propertyExceptions = watcher.propertyExceptions;
        }
        
        protected virtual void watcherChanged(TECChangedEventArgs args) { }
        protected virtual void watcherCostChanged(CostBatch batch) { }
        protected virtual void watcherPointChanged(int pointNum) { }
        protected virtual void watcherPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args) { }
    }

    public class ScopeWatcherFilter : BaseWatcherFilter
    {
        /// <summary>
        /// Adds, removes, edit raised from models, omitting property exceptions and catalogs
        /// </summary>
        public event Action<TECChangedEventArgs> ScopeChanged;

        public ScopeWatcherFilter(ChangeWatcher watcher) : base(watcher) { }

        protected override void watcherChanged(TECChangedEventArgs args)
        {
            if (!propertyExceptions.Contains(args.PropertyName) && !(args.Sender is TECCatalogs))
            {
                this.ScopeChanged?.Invoke(args);
            }
        }
    }

    public class InstanceWatcherFilter : BaseWatcherFilter
    {
        /// <summary>
        /// All Changed events where the object is not typical
        /// </summary>
        public event Action<TECChangedEventArgs> InstanceChanged;

        public InstanceWatcherFilter(ChangeWatcher watcher) : base(watcher) { }

        protected override void watcherChanged(TECChangedEventArgs args)
        {
            if (!propertyExceptions.Contains(args.PropertyName) && !(args.Sender is TECCatalogs))
            {
                if(args.Value is ITypicalable valueTyp)
                {
                    if (!valueTyp.IsTypical)
                    {
                        this.InstanceChanged?.Invoke(args);

                    }
                }
                else
                {
                    if(!(args.Sender is ITypicalable) ||
                        args.Sender is ITypicalable senderTyp && !senderTyp.IsTypical)
                    {
                        this.InstanceChanged?.Invoke(args);
                    }
                }
            }
        }
    }

    public class InstanceConstituentChangedFilter
    {
        /// <summary>
        /// Add, Remove events from all instances and their PropertyObjects
        /// </summary>
        public event Action<Change, ITECObject> InstanceConstituentChanged;

        public InstanceConstituentChangedFilter(ChangeWatcher watcher)
        {
            new InstanceWatcherFilter(watcher).InstanceChanged += instanceFilterChanged;
        }

        private void instanceFilterChanged(TECChangedEventArgs e)
        {
            if ((e.Change == Change.Add || e.Change == Change.Remove) && e.Sender is IRelatable parent)
            {
                if (!parent.LinkedObjects.Contains(e.PropertyName))
                    raiseConstituents(e.Change, e.Value as ITECObject);
            }
        }
        private void raiseConstituents(Change change, ITECObject item)
        {
            InstanceConstituentChanged?.Invoke(change, item);
            if (item is IRelatable parent)
            {
                foreach (var child in parent.GetDirectChildren())
                {
                    raiseConstituents(change, child);
                }
            }
        }
    }

    public class TypicalWatcherFilter : BaseWatcherFilter
    {
        /// <summary>
        /// All Changed events where the object is typical
        /// </summary>
        public event Action<TECChangedEventArgs> TypicalChanged;

        public TypicalWatcherFilter(ChangeWatcher watcher) : base(watcher) { }

        protected override void watcherChanged(TECChangedEventArgs args)
        {
            if (!propertyExceptions.Contains(args.PropertyName) && !(args.Sender is TECCatalogs) &&
                (args.Sender.IsTypical() || args.Value.IsTypical()))
            {
                this.TypicalChanged?.Invoke(args);
            } 
        }
    }

    public class TypicalConsituentChangedFilter
    {
        /// <summary>
        /// Add, Remove events from all typical objects and their PropertyObjects
        /// </summary>
        public event Action<Change, ITECObject> TypicalConstituentChanged;

        public TypicalConsituentChangedFilter(ChangeWatcher watcher)
        {
            new TypicalWatcherFilter(watcher).TypicalChanged += typicalFilterChanged;
        }

        private void typicalFilterChanged(TECChangedEventArgs e)
        {
            if ((e.Change == Change.Add || e.Change == Change.Remove) && e.Sender is IRelatable parent)
            {
                if (!parent.LinkedObjects.Contains(e.PropertyName))
                    raiseTypicalConstituents(e.Change, e.Value as ITECObject);
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
    }

    public class DirectRelationshipChangedFilter
    {
        public event Action<TECChangedEventArgs> DirectRelationshipChanged;

        public DirectRelationshipChangedFilter(ChangeWatcher watcher)
        {
            new ScopeWatcherFilter(watcher).ScopeChanged += scopeFilterChanged;
        }

        private void scopeFilterChanged(TECChangedEventArgs obj)
        {
            if (obj.Value is ITECObject tecObj && ((IRelatable)obj.Sender).IsDirectDescendant(tecObj))
            {
                DirectRelationshipChanged?.Invoke(obj);
            }
        }
    }

    internal static class TypicalExtension
    {
        public static bool IsTypical(this object obj)
        {
            if (obj is ITypicalable typ)
            {
                return typ.IsTypical;
            }
            else
            {
                return false;
            }
        }
    }
}