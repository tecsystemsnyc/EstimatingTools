using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface IControllerConnection : IConnection, ITypicalable, INotifyCostChanged
    {
        Guid Guid { get; }
        TECController ParentController { get; }
        /// <summary>
        /// The IO that will take up space on the controller, be it Protocol connection types or BMS Points.
        /// </summary>
        IOCollection IO { get; }
    }
}
