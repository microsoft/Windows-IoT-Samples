using System.Runtime.InteropServices;

namespace DualOperator.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RawKeyboard
    {
        public ushort Makecode;                 // Scan code from the key depression
        public ushort Flags;                    // One or more of RI_KEY_MAKE, RI_KEY_BREAK, RI_KEY_E0, RI_KEY_E1
        private readonly ushort Reserved;       // Always 0    
        public ushort VKey;                     // Virtual Key Code
        public uint Message;                    // Corresponding Windows message for example (WM_KEYDOWN, WM_SYASKEYDOWN etc)
        public uint ExtraInformation;           // The device-specific addition information for the event (seems to always be zero for keyboards)

        public override string ToString()
        {
            return $"Rawkeyboard\n Makecode: {Makecode}\n Makecode(hex) : {Flags:X}\n Flags: {Flags}\n Reserved: {Reserved}\n VKeyName: {VKey}\n Message: {Message}\n ExtraInformation {ExtraInformation}\n";
        }
    }
}
