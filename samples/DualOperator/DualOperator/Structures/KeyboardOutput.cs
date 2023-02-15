using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardOutput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
