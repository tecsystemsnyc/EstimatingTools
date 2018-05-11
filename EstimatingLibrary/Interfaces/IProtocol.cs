using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface IProtocol
    {
        String Name { get; }
        ObservableCollection<TECConnectionType> ConnectionTypes { get; }
    }
}
