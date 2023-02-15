using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareOutput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}
