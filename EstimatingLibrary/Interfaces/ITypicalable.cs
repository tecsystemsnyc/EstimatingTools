using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;

namespace EstimatingLibrary.Interfaces
{
    public interface ITypicalable
    {
        bool IsTypical { get; }
        ITECObject CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary = null);
        void AddChildForProperty(String property, ITECObject item);
        bool RemoveChildForProperty(String property, ITECObject item);
        bool ContainsChildForProperty(String property, ITECObject item);

        void MakeTypical();
    }
    
}
