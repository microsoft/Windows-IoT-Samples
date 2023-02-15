using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseOutput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
