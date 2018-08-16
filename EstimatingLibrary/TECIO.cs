using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EstimatingLibrary
{
    public enum IOType
    {
        AI,
        AO,
        DI,
        DO,
        UI,
        UO,
        Protocol
    }

    public class TECIO : TECObject, IRelatable, ICatalogContainer
    {
        public static List<IOType> PointIO = new List<IOType>()
        {
            IOType.AI, IOType.AO, IOType.DI, IOType.DO
        };
        public static List<IOType> UniversalIO = new List<IOType>()
        {
            IOType.UI, IOType.UO
        };
        public static List<IOType> ControllerIO = PointIO.Concat(UniversalIO).ToList();

        public static IOType GetUniversalType(IOType type)
        {
            if (PointIO.Contains(type))
            {
                if (type == IOType.AI || type == IOType.DI)
                {
                    return IOType.UI;
                }
                else if (type == IOType.AO || type == IOType.DO)
                {
                    return IOType.UO;
                }
                else
                {
                    throw new NotImplementedException("PointIO type not recognized.");
                }
            }
            else
            {
                throw new InvalidOperationException("Universal type must come from point type.");
            }
        }

        #region Properties
        private IOType _type;
        public IOType Type
        {
            get { return _type; }
            set
            {
                if(value is IOType.Protocol)
                {
                    throw new Exception("Type cannot be set to Protocol explicitly.");
                }
                var old = Type;
                _type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old);
                Protocol = null;
            }
        }

        private TECProtocol _protocol;
        public TECProtocol Protocol
        {
            get { return _protocol; }
            set
            {
                var old = Protocol;
                _protocol = value;
                notifyCombinedChanged(Change.Edit, "Protocol", this, value, old);
                if(value != null)
                {
                    setTypeToProtocol();
                }
            }
        }
        
        private int _quantity = 1;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                var old = Quantity;
                _quantity = value;
                notifyCombinedChanged(Change.Edit, "Quantity", this, value, old);
            }
        }

        public string DisplayName
        {
            get
            {
                if (this.Protocol != null)
                {
                    return this.Protocol.Label;
                }
                else
                {
                    return this.Type.ToString();
                }
            }
        }
        
        private void setTypeToProtocol()
        {
            var old = Type;
            _type = IOType.Protocol;
            notifyCombinedChanged(Change.Edit, "Type", this, Type, old);
        }

        #endregion
        
        public TECIO(Guid guid, IOType type) : base(guid)
        {
            _type = type;
        }
        public TECIO(Guid guid, TECProtocol protocol) : this(guid, IOType.Protocol)
        {
            _protocol = protocol ?? throw new Exception();
        }

        public TECIO(IOType type) : this(Guid.NewGuid(), type) { }
        public TECIO(TECProtocol protocol) : this(Guid.NewGuid(), protocol) { }

        public TECIO(TECIO ioSource) : this(ioSource.Type)
        {
            _quantity = ioSource.Quantity;
            _protocol = ioSource.Protocol;
        }

        #region Relatable

        RelatableMap IRelatable.PropertyObjects
        {
            get
            {
                RelatableMap map = new RelatableMap();
                if(Protocol != null) map.Add(Protocol, "Protocol");
                return map;
            }
        }

        RelatableMap IRelatable.LinkedObjects
        {
            get
            {
                RelatableMap map = new RelatableMap();
                if (Protocol != null) map.Add(Protocol, "Protocol");
                return map;
            }
        }
        #endregion

        #region ICatalogContainer
        public bool RemoveCatalogItem<T>(T item, T replacement) where T : class, ICatalog
        {
            bool replacedProt = false;
            if (item == this.Protocol)
            {
                if (replacement is TECProtocol prot)
                {
                    this.Protocol = prot;
                }
                else throw new ArgumentException("Replacement Protocol cannot be null.");
            }
            return (replacedProt);
        }
        #endregion
    }

    public class IOCollection
    {
        private Dictionary<IOType, TECIO> ioDictionary = new Dictionary<IOType, TECIO>();
        private Dictionary<IProtocol, TECIO> protocolDictionary = new Dictionary<IProtocol, TECIO>();

        public List<IProtocol> Protocols
        {
            get
            {
                List<IProtocol> protocols = new List<IProtocol>();
                foreach(TECIO io in this.protocolDictionary.Values)
                {
                    if (io.Protocol == null) throw new Exception("TECIO exists in protocol dictionary that doesn't have protocol.");
                    for(int i = 0; i < io.Quantity; i++)
                    {
                        protocols.Add(io.Protocol);
                    }
                }
                return protocols;
            }
        }
        public IOCollection PointIO
        {
            get
            {
                return new IOCollection(ioDictionary.Values);
            }
        }

        public IOCollection() { }
        public IOCollection(IEnumerable<TECIO> io) : this()
        {
            Add(io);
        }
        public IOCollection(IOCollection collection) : this()
        {
            Add(collection.ToList());
        }

        public List<TECIO> ToList()
        {
            List<TECIO> list = new List<TECIO>();
            list.AddRange(ioDictionary.Values);
            list.AddRange(protocolDictionary.Values);
            return list;
        }
        public int TypeCount()
        {
            return this.ToList().Count();
        }
        
        public bool Contains(IOType type)
        {
            return this.Contains(new TECIO(type));
        }
        public bool Contains(TECProtocol protocol)
        {
            return this.Contains(new TECIO(protocol));
        }
        public bool Contains(TECIO io)
        {
            if (TECIO.UniversalIO.Contains(io.Type))
            {
                return ioDictionary.ContainsKey(io.Type) ? ioDictionary[io.Type].Quantity >= io.Quantity : false;
            }
            else if (io.Protocol != null)
            {
                return protocolDictionary.ContainsKey(io.Protocol) ? protocolDictionary[io.Protocol].Quantity >= io.Quantity : false;
            }
            else if (TECIO.PointIO.Contains(io.Type))
            {
                IOType universalType = TECIO.GetUniversalType(io.Type);
                int quantity = ioDictionary.ContainsKey(io.Type) ? ioDictionary[io.Type].Quantity : 0;
                quantity += ioDictionary.ContainsKey(universalType) ? ioDictionary[universalType].Quantity : 0;
                return quantity >= io.Quantity;
            }
            else
            {
                throw new Exception("IO condition not recognized.");
            }
        }
        public bool Contains(IEnumerable<TECIO> io)
        {
            //Normalize collections
            IOCollection thisCollection = new IOCollection(this);
            IOCollection ioCollection = new IOCollection(io);

            foreach (TECIO ioToCheck in ioCollection.ToList())
            {
                if (!thisCollection.Remove(ioToCheck)) return false;
            }
            return true;
        }
        public bool Contains(IOCollection io)
        {
            return (this.Contains(io.ToList()));
        }
        public void Add(IOType type)
        {
            this.Add(new TECIO(type));
        }
        public void Add(TECProtocol protocol)
        {
            this.Add(new TECIO(protocol));
        }
        public void Add(TECIO io)
        {
            if(io.Type == IOType.Protocol)
            {
                if (protocolDictionary.ContainsKey(io.Protocol))
                {
                    protocolDictionary[io.Protocol].Quantity += io.Quantity;
                }
                else
                {
                    protocolDictionary.Add(io.Protocol, new TECIO(io));
                }
            }
            else
            {
                if (ioDictionary.ContainsKey(io.Type))
                {
                    ioDictionary[io.Type].Quantity += io.Quantity;
                }
                else
                {
                    ioDictionary.Add(io.Type, new TECIO(io));
                }
            }
            
        }
        public void Add(IEnumerable<TECIO> ioList)
        {
            foreach(TECIO io in ioList)
            {
                Add(io);
            }
        }
        public bool Remove(TECIO io)
        {
            if (!this.Contains(io)) return false;
            if(io.Type == IOType.Protocol)
            {
                for(int x = 0; x < io.Quantity; x++)
                {
                    remove(io.Protocol);
                }
            }
            else
            {
                for(int x = 0; x < io.Quantity; x++)
                {
                    remove(io.Type);
                }
            }
            return true;
        }
        public bool Remove(IEnumerable<TECIO> ioList)
        {
            if (this.Contains(ioList))
            {
                foreach (TECIO io in ioList)
                {
                    if (!Remove(io)) throw new Exception("This IO Collection is corrupt. Could not remove TECIO from collection previously confirmed as containing TECIO.");
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Remove(IOCollection ioCollection)
        {
            return this.Remove(ioCollection.ToList());
        }

        public static IOCollection operator +(IOCollection left, IOCollection right)
        {
            IOCollection newCollection = new IOCollection(left);
            newCollection.Add(right.ToList());
            return newCollection;
        }

        /// <summary>
        /// Gives the intersection of the left and right IOCollections
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The intersection</returns>
        public static IOCollection operator |(IOCollection left, IOCollection right)
        {
            IOCollection intersection = new IOCollection();
            foreach(TECIO leftIO in left.ToList())
            {
                foreach(TECIO rightIO in right.ToList())
                {
                    if (leftIO.Type == rightIO.Type)
                    {
                        intersection.Add(leftIO.Quantity < rightIO.Quantity ? leftIO : rightIO); 
                    }
                }
            }
            return intersection;
        }
        
        private bool remove(IOType type)
        {
            if (type == IOType.Protocol)
            {
                throw new Exception("Cannot remove Protocol as IOType");
            }
            if (ioDictionary.ContainsKey(type))
            {
                TECIO io = ioDictionary[type];
                io.Quantity--;
                if (io.Quantity < 1)
                {
                    ioDictionary.Remove(io.Type);
                }
                return true;
            }
            else
            {
                if (TECIO.PointIO.Contains(type))
                {
                    IOType universalType = TECIO.GetUniversalType(type);
                    if (ioDictionary.ContainsKey(universalType))
                    {
                        TECIO universalIO = ioDictionary[universalType];
                        universalIO.Quantity--;
                        if (universalIO.Quantity < 1)
                        {
                            ioDictionary.Remove(universalIO.Type);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        private bool remove(TECProtocol protocol)
        {
            if (protocolDictionary.ContainsKey(protocol))
            {
                TECIO io = protocolDictionary[protocol];
                io.Quantity--;
                if (io.Quantity < 1)
                {
                    protocolDictionary.Remove(io.Protocol);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class IOCollectionExtensions
    {
        public static IOCollection ToIOCollection(this IEnumerable<TECIOModule> modules)
        {
            IOCollection io = new IOCollection();
            modules.ForEach(mod => io.Add(mod.IO));
            return io;
        }

        public static IOCollection ToIOCollection(this TECIO io)
        {
            return new IOCollection(new List<TECIO>() { io });
        }

        public static IOCollection ToIOCollection(this IEnumerable<TECPoint> points)
        {
            IOCollection collection = new IOCollection();
            foreach (TECPoint point in points)
            {
                TECIO toAdd = new TECIO(point.Type);
                toAdd.Quantity = point.Quantity;
                collection.Add(toAdd);
            }
            return collection;
        }

        public static IOCollection ToIOCollection(this TECProtocol protocol)
        {
            return new IOCollection(new List<TECIO> { new TECIO(protocol) });
        }
    }
}
