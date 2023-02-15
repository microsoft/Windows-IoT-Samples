using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RawHID
    {
        public uint dwSizHid;
        public uint dwCount;
        public byte bRawData;

        public override string ToString()
        {
            return $"Rawhib\n dwSizeHid : {dwSizHid}\n dwCount : {dwCount}\n bRawData : {bRawData}\n";
        }
    }
}
