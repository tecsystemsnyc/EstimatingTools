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

    public class TECIO : TECObject
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
                var old = Type;
                _type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old);
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

        #endregion
        
        public TECIO(Guid guid, IOType type) : base(guid)
        {
            _type = type;
        }
        public TECIO(Guid guid, TECProtocol protocol) : this(guid, IOType.Protocol)
        {
            _protocol = protocol;
        }

        public TECIO(IOType type) : this(Guid.NewGuid(), type) { }
        public TECIO(TECProtocol protocol) : this(Guid.NewGuid(), protocol) { }

        public TECIO(TECIO ioSource) : this(ioSource.Type)
        {
            _quantity = ioSource.Quantity;
            _protocol = ioSource.Protocol;
        }
    }

    public class IOCollection
    {
        private Dictionary<IOType, TECIO> ioDictionary = new Dictionary<IOType, TECIO>();
        private Dictionary<TECProtocol, TECIO> protocolDictionary = new Dictionary<TECProtocol, TECIO>();

        public IOCollection() { }
        public IOCollection(IEnumerable<TECIO> io) : this()
        {
            AddIO(io);
        }
        public IOCollection(IOCollection collection) : this()
        {
            foreach(TECIO io in collection.ListIO())
            {
                AddIO(io);
            }
        }

        public List<TECIO> ListIO()
        {
            List<TECIO> list = new List<TECIO>();
            list.AddRange(ioDictionary.Values);
            list.AddRange(protocolDictionary.Values);
            return list;
        }
        public bool Contains(IOType type)
        {
            if (ioDictionary.ContainsKey(type))
            {
                return true;
            }
            else
            {
                return ioDictionary.ContainsKey(TECIO.GetUniversalType(type));
            }
        }
        public bool Contains(TECIO io)
        {
            if(io.Protocol != null)
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
                return false;
            }
        }
        public bool Contains(IEnumerable<TECIO> io)
        {
            //Normalize collections
            IOCollection thisCollection = new IOCollection(this);
            IOCollection ioCollection = new IOCollection(io);

            foreach (TECIO ioToCheck in ioCollection.ListIO())
            {
                if (thisCollection.Contains(ioToCheck))
                {
                    thisCollection.RemoveIO(ioToCheck);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public bool Contains(IOCollection io)
        {
            return (this.Contains(io.ListIO()));
        }
        public void AddIO(IOType type)
        {
            if(type == IOType.Protocol)
            {
                throw new Exception("Cannot add .Protocol as IOType");
            }
            if (ioDictionary.ContainsKey(type))
            {
                ioDictionary[type].Quantity++;
            }
            else
            {
                TECIO io = new TECIO(type);
                ioDictionary.Add(type, io);
            }
        }
        public void AddIO(TECProtocol protocol)
        {
            if (protocolDictionary.ContainsKey(protocol))
            {
                protocolDictionary[protocol].Quantity++;
            }
            else
            {
                TECIO io = new TECIO(protocol);
                protocolDictionary.Add(protocol, io);
            }
        }
        public void AddIO(TECIO io)
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
        public void AddIO(IEnumerable<TECIO> ioList)
        {
            foreach(TECIO io in ioList)
            {
                AddIO(io);
            }
        }
        public void RemoveIO(IOType type)
        {
            if (type == IOType.Protocol)
            {
                throw new Exception("Cannot remove .Protocol as IOType");
            }
            if (ioDictionary.ContainsKey(type))
            {
                TECIO io = ioDictionary[type];
                io.Quantity--;
                if (io.Quantity < 1)
                {
                    ioDictionary.Remove(io.Type);
                }
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
                    }
                    else
                    {
                        throw new InvalidOperationException("IOCollection does not contain IOType.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("IOCollection does not contain IOType.");
                }
            }
        }
        public void RemoveIO(TECProtocol protocol)
        {
            if (protocolDictionary.ContainsKey(protocol))
            {
                TECIO io = protocolDictionary[protocol];
                io.Quantity--;
                if (io.Quantity < 1)
                {
                    protocolDictionary.Remove(io.Protocol);
                }
            }
            else
            {
                throw new InvalidOperationException("IOCollection does not contain Protocol.");
            }
        }
        public void RemoveIO(TECIO io)
        {
            if(io.Type == IOType.Protocol)
            {
                for(int x = 0; x < io.Quantity; x++)
                {
                    RemoveIO(io.Protocol);
                }
            }
            else
            {
                for(int x = 0; x < io.Quantity; x++)
                {
                    RemoveIO(io.Type);
                }
            }
        }
        public void RemoveIO(IEnumerable<TECIO> ioList)
        {
            if (this.Contains(ioList))
            {
                foreach (TECIO io in ioList)
                {
                    RemoveIO(io);
                }
            }
            else
            {
                throw new InvalidOperationException("IOCollection does not contain enough IO.");
            }
        }

        public static IOCollection operator +(IOCollection left, IOCollection right)
        {
            IOCollection newCollection = new IOCollection(left);
            newCollection.AddIO(right.ListIO());
            return newCollection;
        }
        public static IOCollection operator -(IOCollection left, IOCollection right)
        {
            IOCollection newCollection = new IOCollection(left);
            newCollection.RemoveIO(right.ListIO());
            return newCollection;
        }

        public static bool IOTypesMatch(IOCollection collection1, IOCollection collection2)
        {
            foreach(KeyValuePair<IOType, TECIO> pair in collection1.ioDictionary)
            {
                if (!collection2.Contains(pair.Key)) return false;
            }
            foreach(KeyValuePair<IOType, TECIO> pair in collection2.ioDictionary)
            {
                if (!collection1.Contains(pair.Key)) return false;
            }
            return true;
        }
    }
}
