using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface ICatalogContainer : ITECObject
    {
        bool RemoveCatalogItem<T>(T item, T replacement) where T : class, ICatalog;
    }
}
