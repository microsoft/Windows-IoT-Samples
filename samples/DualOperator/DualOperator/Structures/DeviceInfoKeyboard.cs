namespace DualOperator.Structures
{
    public struct DeviceInfoKeyboard
    {
        public uint Type;                       // Type of the keyboard
        public uint SubType;                    // Subtype of the keyboard
        public uint KeyboardMode;               // The scan code mode
        public uint NumberOfFunctionKeys;       // Number of function keys on the keyboard
        public uint NumberOfIndicators;         // Number of LED indicators on the keyboard
        public uint NumberOfKeysTotal;          // Total number of keys on the keyboard

        public override string ToString()
        {
            return $"DeviceInfoKeyboard\n Type: {Type}\n SubType: {SubType}\n KeyboardMode: {KeyboardMode}\n NumberOfFunctionKeys: {NumberOfFunctionKeys}\n NumberOfIndicators {NumberOfIndicators}\n NumberOfKeysTotal: {NumberOfKeysTotal}\n";
        }
    }
}
