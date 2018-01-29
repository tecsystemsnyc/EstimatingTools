using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.ViewModels
{
    public class RiserVM : ViewModelBase
    {

        private TECBid _bid;

        public TECBid Bid
        {
            get { return _bid; }
            set
            {
                _bid = value;
                RaisePropertyChanged("Bid");
            }
        }

        public RiserVM(TECBid bid)
        {
            _bid = bid;
            
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

    public class LocationList : IEnumerable<LocationContainer>
    {
        List<LocationContainer> locations = new List<LocationContainer>();

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

            if (!locations.Any(item => item.Location == located.Location))
            {
                locations.Add(new LocationContainer(located.Location, new List<TECLocated> { located }));
            } else
            {

            }
        }
    }
}
