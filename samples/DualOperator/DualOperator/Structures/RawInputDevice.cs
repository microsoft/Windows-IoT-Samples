using System.Runtime.InteropServices;
using DualOperator.Enumerations;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RawInputDevice
    {
        internal HIDUsagePage UsagePage;
        internal HIDUsage Usage;
        internal RawInputDeviceFlags Flags;
        internal IntPtr Target;

        public override string ToString()
        {
            return $"{UsagePage}/{Usage}, flags: {Flags}, target: {Target}";
        }
    }
}
