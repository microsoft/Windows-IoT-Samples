using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Explicit)]
    public struct OutputUnion
    {
        [FieldOffset(0)] public MouseOutput mi;
        [FieldOffset(0)] public KeyboardOutput ki;
        [FieldOffset(0)] public HardwareOutput hi;
    }
}
