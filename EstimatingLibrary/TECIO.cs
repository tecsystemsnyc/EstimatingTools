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

        public IOCollection Protocols
        {
            get
            {
                return new IOCollection(protocolDictionary.Values);
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
            Add(collection.ListIO());
        }

        public List<TECIO> ToList()
        {
            List<TECIO> list = new List<TECIO>();
            list.AddRange(ioDictionary.Values);
            list.AddRange(protocolDictionary.Values);
            return list;
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

            foreach (TECIO ioToCheck in ioCollection.ListIO())
            {
                if (thisCollection.Contains(ioToCheck))
                {
                    thisCollection.Remove(ioToCheck);
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
        public void Remove(TECIO io)
        {
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
        }
        public void Remove(IEnumerable<TECIO> ioList)
        {
            if (this.Contains(ioList))
            {
                foreach (TECIO io in ioList)
                {
                    Remove(io);
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
            newCollection.Add(right.ListIO());
            return newCollection;
        }
        public static IOCollection operator -(IOCollection left, IOCollection right)
        {
            IOCollection newCollection = new IOCollection(left);
            newCollection.Remove(right.ListIO());
            return newCollection;
        }
        
        private void remove(IOType type)
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
        private void remove(TECProtocol protocol)
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
    }
}
