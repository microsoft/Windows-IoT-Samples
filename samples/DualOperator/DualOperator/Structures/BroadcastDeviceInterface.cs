using DualOperator.Enumerations;

namespace DualOperator.Structures
{
    struct BroadcastDeviceInterface
    {
        public Int32 DbccSize;
        public BroadcastDeviceType BroadcastDeviceType;
        public Int32 DbccReserved;
        public Guid DbccClassguid;
        public char DbccName;
    }
}
