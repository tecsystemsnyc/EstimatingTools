using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class RiserVM : ViewModelBase, IDropTarget
    {
        private TECBid bid;

        private ChangeWatcher watcher;
        private LocationList _locations;
        private ObservableCollection<TECLocated> _unlocated;
        private TECLocated _selected;

        private string _newLocationName = "";
        private string _newLocationTag = "";

        private string _patternName = "";
        private int _patternStart = 0;
        private int _patternEnd = 0;

        public LocationList Locations
        {
            get { return _locations; }
            set
            {
                _locations = new LocationList();
                RaisePropertyChanged("Locations");
            }
        }
        public ObservableCollection<TECLocated> Unlocated
        {
            get { return _unlocated; }
            set
            {
                _unlocated = value;
                RaisePropertyChanged("Unlocated");
            }
        }
        public TECLocated Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
                PropertiesVM.Selected = value;
            }
        }
        public PropertiesVM PropertiesVM { get; set; }

        public string NewLocationName
        {
            get { return _newLocationName; }
            set
            {
                if (_newLocationName != value)
                {
                    _newLocationName = value;
                    RaisePropertyChanged("NewLocationName");
                }
            }
        }
        public string NewLocationTag
        {
            get { return _newLocationTag; }
            set
            {
                if (_newLocationTag != value)
                {
                    _newLocationTag = value;
                    RaisePropertyChanged("NewLocationTag");
                }
            }
        }
        
        public string PatternName
        {
            get { return _patternName; }
            set
            {
                _patternName = value;
                RaisePropertyChanged("PatternName");
            }
        }
        public int PatternStart
        {
            get { return _patternStart; }
            set
            {
                _patternStart = value;
                RaisePropertyChanged("PatternStart");
            }
        }
        public int PatternEnd
        {
            get { return _patternEnd; }
            set
            {
                _patternEnd = value;
                RaisePropertyChanged("PatternEnd");
            }
        }
        
        public ICommand AddLocationCommand { get; private set; }
        public ICommand AddPatternCommand { get; private set; }
        public RelayCommand<LocationContainer> DeleteCommand { get; private set; }
        
        public RiserVM(TECBid bid, ChangeWatcher watcher)
        {
            this.bid = bid;
            this.watcher = watcher;
            this.watcher.Changed += changed;
            populateBidLocations(bid);
            AddLocationCommand = new RelayCommand(addLocationExecute, canAddLocation);
            AddPatternCommand = new RelayCommand(addPatternExecute, canAddPattern);
            PropertiesVM = new PropertiesVM(bid.Catalogs, bid);
            DeleteCommand = new RelayCommand<LocationContainer>(deleteLocationExecute, canDeleteLocation);
        }

        private void deleteLocationExecute(LocationContainer obj)
        {
            Locations.Remove(obj.Location);
        }

        private bool canDeleteLocation(LocationContainer arg)
        {
            return true;
        }

        private void addPatternExecute()
        {
            for(int x = PatternStart; x <= PatternEnd; x++)
            {
                TECLocation newLocation = new TECLocation();
                newLocation.Name = String.Format("{0} {1}", PatternName, x);
                newLocation.Label = x.ToString();
                bid.Locations.Add(newLocation);
            }
        }
        private bool canAddPattern()
        {
            return PatternEnd > PatternStart;
        }
        
        private void addLocationExecute()
        {
            TECLocation newLocation = new TECLocation();
            newLocation.Name = NewLocationName;
            newLocation.Label = NewLocationTag;
            bid.Locations.Add(newLocation);
            NewLocationName = "";
            NewLocationTag = "";
        }
        private bool canAddLocation()
        {
            return (NewLocationName != "");
        }

        private void changed(TECChangedEventArgs obj)
        {
            if (obj.PropertyName == "Locations" && obj.Value is TECLocation location)
            {
                if (obj.Change == Change.Add)
                {
                    Locations.Add(location);
                }
                else if (obj.Change == Change.Remove)
                {
                    Locations.Remove(location);
                }
            }
            else if (obj.PropertyName == "Location" && obj.Sender is TECLocated located && passesPredicate(obj.Sender))
            {
                Locations.Move(located, obj.OldValue as TECLabeled, obj.Value as TECLabeled);
                if(obj.OldValue == null && obj.Value != null)
                {
                    Unlocated.Remove(located);
                } else if(obj.OldValue != null && obj.Value == null)
                {
                    Unlocated.Add(located);
                }
            }
            else if (obj.Value is TECTypical typical)
            {
                foreach (TECSystem instance in typical.Instances)
                {
                    if (obj.Change == Change.Add)
                    {
                        addLocated(instance);
                    }
                    else if (obj.Change == Change.Remove)
                    {
                        removeLocated(instance);
                    }
                }
            }
            else if (obj.Value is TECSystem item)
            {
                if (obj.Change == Change.Add)
                {
                    addLocated(item);
                }
                else if (obj.Change == Change.Remove)
                {
                    removeLocated(item);
                }
            }
            else if (obj.Value is TECController controller && obj.Sender is TECBid)
            {
                if (obj.Change == Change.Add)
                {
                    addLocated(controller);
                }
                else if (obj.Change == Change.Remove)
                {
                    removeLocated(controller);
                }
            }
        }

        private bool passesPredicate(ITECObject sender)
        {
            return sender is TECSystem || sender is TECController;
        }

        private void addLocated(TECLocated system)
        {
            if (system.Location != null)
            {
                Locations.Add(system);
            }
            else
            {
                Unlocated.Add(system);
            }
        }
        private void removeLocated(TECLocated system)
        {
            if (system.Location != null)
            {
                Locations.Remove(system);
            }
            else
            {
                Unlocated.Remove(system);
            }
        }
        private void populateBidLocations(TECBid bid)
        {
            Locations = new LocationList();
            Unlocated = new ObservableCollection<TECLocated>();
            foreach(TECLocation label in bid.Locations)
            {
                Locations.Add(label);
            }
            foreach(TECTypical typical in bid.Systems)
            {
                foreach(TECSystem system in typical.Instances)
                {
                    addLocated(system);
                }
            }
            foreach(TECController controller in bid.Controllers)
            {
                addLocated(controller);
            }
            Locations.CollectionChanged += Locations_CollectionChanged;
        }

        private void Locations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Move)
            {
                bid.Locations.Move(e.OldStartingIndex, e.NewStartingIndex);
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragDropHelpers.DragOver(dropInfo, dropCondition, null);

            bool dropCondition(object data, Type sourceType, Type targetType)
            {
                bool dataComplies = typeof(TECLocated).IsAssignableFrom(sourceType);
                bool targetComplies = Locations.Any(item => item.Scope == dropInfo.TargetCollection) ||
                    dropInfo.TargetCollection == Unlocated;
                bool isReorder = sourceType == typeof(LocationContainer) && targetType == typeof(LocationContainer);
                return (dataComplies && targetComplies) || isReorder;
            }

        }
        public void Drop(IDropInfo dropInfo)
        {
            DragDropHelpers.Drop(dropInfo, dropMethod, false);
            object dropMethod(object dropped)
            {
                if (dropInfo.TargetCollection == Unlocated)
                {
                    ((TECLocated)dropInfo.Data).Location = null;

                }
                else
                {
                    var container = Locations.First(item => item.Scope == dropInfo.TargetCollection);
                    ((TECLocated)dropInfo.Data).Location = container.Location;
                }
                return true;
            }
        }
    }

    public class LocationContainer
    {
        public TECLocation Location { get; }
        public ObservableCollection<TECLocated> Scope { get; }

        public LocationContainer(TECLocation location, IEnumerable<TECLocated> scope)
        {
            Location = location;
            Scope = new ObservableCollection<TECLocated>(scope);
        }
    }
    public class LocationList : IEnumerable<LocationContainer>, IList, INotifyCollectionChanged
    {
        ObservableCollection<LocationContainer> locations = new ObservableCollection<LocationContainer>();
        
        bool IList.IsReadOnly => ((IList)locations).IsReadOnly;

        bool IList.IsFixedSize => ((IList)locations).IsFixedSize;

        int ICollection.Count => ((IList)locations).Count;

        object ICollection.SyncRoot => ((IList)locations).SyncRoot;

        bool ICollection.IsSynchronized => ((IList)locations).IsSynchronized;

        object IList.this[int index] { get => ((IList)locations)[index]; set => ((IList)locations)[index] = value; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public LocationList()
        {
            locations.CollectionChanged += (sender, e) =>
            {
                CollectionChanged?.Invoke(sender, e);
            };
        }
        
        public IEnumerator<LocationContainer> GetEnumerator()
        {
            return locations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(TECLocated located)
        {
            LocationContainer container = locations.First(item => item.Location == located.Location);
            
            if(container != null)
            {
                container.Scope.Add(located);
            }
            else
            {
                locations.Add(new LocationContainer(located.Location, new List<TECLocated> { located }));
            }
        }

        public void Add(TECLocation location)
        {
            if(!locations.Any(item => item.Location == location))
            {
                locations.Add(new LocationContainer(location, new List<TECLocated>()));
            }
        }
        
        public void Dislocate(TECLocated located)
        {
            Remove(located);
            located.Location = null;
        }

        public void Remove(TECLocated located)
        {
            foreach(LocationContainer container in locations)
            {
                if (container.Scope.Contains(located))
                {
                    container.Scope.Remove(located);
                }
            }
            
        }

        public void Remove(TECLabeled location)
        {
            LocationContainer container = locations.First(item => item.Location == location);
            foreach(TECLocated item in container.Scope)
            {
                item.Location = null;
            }
            locations.Remove(container);
        }

        public void Move(TECLocated located, TECLabeled orginalLocation, TECLabeled newLocation)
        {
            foreach(LocationContainer container in locations)
            {
                if(container.Location == orginalLocation)
                {
                    container.Scope.Remove(located);
                    break;
                }
            }
            foreach(LocationContainer container in locations)
            {
                if(container.Location == newLocation)
                {
                    container.Scope.Add(located);
                    break;
                }
            }
        }

        public void Move(int currentIndex, int finalIndex)
        {
            locations.Move(currentIndex, finalIndex);
        }

        public int IndexOf(LocationContainer container)
        {
            return locations.IndexOf(container);
        }
        
        int IList.Add(object value)
        {
            return ((IList)locations).Add(value);
        }

        bool IList.Contains(object value)
        {
            return ((IList)locations).Contains(value);
        }

        void IList.Clear()
        {
            ((IList)locations).Clear();
        }

        int IList.IndexOf(object value)
        {
            return ((IList)locations).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)locations).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            ((IList)locations).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            ((IList)locations).RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)locations).CopyTo(array, index);
        }
    }
}
