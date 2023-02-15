using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RawData
    {
        [FieldOffset(0)]
        internal RawMouse mouse;
        [FieldOffset(0)]
        internal RawKeyboard keyboard;
        [FieldOffset(0)]
        internal RawHID hid;
    }
}
