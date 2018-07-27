using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Interfaces
{
    public interface ICatalog<T> : ICatalog
    {
        T CatalogCopy();
    }

    public interface ICatalog { }
}
