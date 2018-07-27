using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface ICatalogContainer : ITECObject
    {
        bool ContainsCatalogItem<T>(T item) where T : class, ICatalog<T>;
        void RemoveCatalogItem<T>(T item, T replacement = null) where T : class, ICatalog<T>;
    }
}
