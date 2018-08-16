using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface IDoRedoable
    {
        bool CanDo { get; }
        void AddForProperty(string property, object item);
        void RemoveForProperty(string property, object item);
        void SetProperty(string property, object value);
    }
}
