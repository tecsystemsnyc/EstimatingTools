using EstimatingLibrary.Interfaces;

namespace EstimatingLibrary.Interfaces
{
    public interface IConnection
    {
        double ConduitLength { get; set; }
        TECElectricalMaterial ConduitType { get; set; }
        bool IsPlenum { get; set; }
        double Length { get; set; }
        IProtocol Protocol { get; }
    }
}