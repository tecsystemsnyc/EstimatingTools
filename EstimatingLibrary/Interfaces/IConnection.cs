using EstimatingLibrary.Interfaces;
using System;

namespace EstimatingLibrary.Interfaces
{
    public interface IConnection : ITECObject, INotifyCostChanged
    {
        Guid Guid { get; }
        double ConduitLength { get; set; }
        TECElectricalMaterial ConduitType { get; set; }
        bool IsPlenum { get; set; }
        double Length { get; set; }
        IProtocol Protocol { get; }
    }

    public static class IConnectionExtensions
    {
        public static void UpdatePropertiesBasedOn(this IConnection toUpdate, IConnection basis)
        {
            toUpdate.Length = basis.Length;
            toUpdate.ConduitLength = basis.ConduitLength;
            toUpdate.ConduitType = basis.ConduitType;
            toUpdate.IsPlenum = basis.IsPlenum;
        }
    }
} 