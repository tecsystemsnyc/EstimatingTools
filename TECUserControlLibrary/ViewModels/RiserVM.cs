using EstimatingLibrary;
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
        private String _locationText = "";
        private TECLocated _selected;   
        
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
        public String LocationText
        {
            get { return _locationText; }
            set
            {
                _locationText = value;
                RaisePropertyChanged("LocationText");
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

        public ICommand AddLocationCommand { get; private set; }
               
        public RiserVM(TECBid bid, ChangeWatcher watcher)
        {
            this.bid = bid;
            this.watcher = watcher;
            this.watcher.Changed += changed;
            populateBidLocations(bid);
            AddLocationCommand = new RelayCommand(addLocationExecute, canAddLocation);
            PropertiesVM = new PropertiesVM(bid.Catalogs, bid);
        }
        public void Refresh(TECBid bid, ChangeWatcher watcher)
        {
            this.bid = bid;
            this.watcher.Changed -= changed;
            this.watcher = watcher;
            this.watcher.Changed += changed;
            populateBidLocations(bid);
            PropertiesVM.Refresh(bid.Catalogs, bid);
        }

        private void addLocationExecute()
        {
            TECLabeled newLocation = new TECLabeled();
            newLocation.Label = LocationText;
            bid.Locations.Add(newLocation);
            LocationText = "";
        }
        private bool canAddLocation()
        {
            return LocationText != "";
        }

        private void changed(TECChangedEventArgs obj)
        {
            if (obj.PropertyName == "Locations" && obj.Value is TECLabeled location)
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
            else if (obj.PropertyName == "Location" && obj.Sender is TECSystem system)
            {
                Locations.Move(system, obj.OldValue as TECLabeled, obj.Value as TECLabeled);
                if(obj.OldValue == null && obj.Value != null)
                {
                    Unlocated.Remove(system);
                } else if(obj.OldValue != null && obj.Value == null)
                {
                    Unlocated.Add(system);
                }
            }
            else if (obj.Value is TECTypical typical)
            {
                foreach (TECSystem instance in typical.Instances)
                {
                    if (obj.Change == Change.Add)
                    {
                        addSystem(instance);
                    }
                    else if (obj.Change == Change.Remove)
                    {
                        removeSystem(instance);
                    }
                }
            }
            else if (obj.Value is TECSystem item)
            {
                if (obj.Change == Change.Add)
                {
                    addSystem(item);
                }
                else if (obj.Change == Change.Remove)
                {
                    removeSystem(item);
                }
            }
            
        }
        private void addSystem(TECSystem system)
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
        private void removeSystem(TECSystem system)
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
            foreach(TECLabeled label in bid.Locations)
            {
                Locations.Add(label);
            }
            foreach(TECTypical typical in bid.Systems)
            {
                foreach(TECSystem system in typical.Instances)
                {
                    addSystem(system);
                }
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            bool dataComplies = dropInfo.Data is TECLocated ||
                (dropInfo.Data is IList sourceList && sourceList.Count > 0 && sourceList[0] is TECLocated);
            bool targetComplies = Locations.Any(item => item.Scope == dropInfo.TargetCollection) ||
                dropInfo.TargetCollection == Unlocated;
            if (targetComplies && dataComplies)
            {
                UIHelpers.SetDragAdorners(dropInfo);
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            if(dropInfo.TargetCollection == Unlocated)
            {
                if (dropInfo.Data is IList sourceList)
                {
                    foreach (TECLocated item in sourceList)
                    {
                        item.Location = null;
                    }
                }
                else
                {
                    ((TECLocated)dropInfo.Data).Location = null;
                }
            }
            else
            {
                var container = Locations.First(item => item.Scope == dropInfo.TargetCollection);
                if (dropInfo.Data is IList sourceList)
                {
                    foreach (TECLocated item in sourceList)
                    {
                        item.Location = container.Location;
                    }
                }
                else
                {
                    ((TECLocated)dropInfo.Data).Location = container.Location;
                }
            }
            

        }
    }

    public class LocationContainer
    {
        public TECLabeled Location { get; }
        public ObservableCollection<TECLocated> Scope { get; }

        public LocationContainer(TECLabeled location, IEnumerable<TECLocated> scope)
        {
            Location = location;
            Scope = new ObservableCollection<TECLocated>(scope);
        }
    }
    public class LocationList : IEnumerable<LocationContainer>, INotifyCollectionChanged
    {
        ObservableCollection<LocationContainer> locations = new ObservableCollection<LocationContainer>();

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

        public void Add(TECLabeled location)
        {
            if(!locations.Any(item => item.Location == location))
            {
                locations.Add(new LocationContainer(location, new List<TECLocated>()));
            }
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
            located.Location = null;
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
    }
}
