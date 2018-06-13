using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EstimatingLibrary.Interfaces
{
    public interface IInterlockable : ITECObject
    {
        ObservableCollection<TECInterlockConnection> Interlocks { get; }
    }
}
