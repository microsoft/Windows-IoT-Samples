namespace DualOperator.Structures
{
    public struct DeviceInfoMouse
    {
        public uint Id;                         // Identifier of the mouse device
        public uint NumberOfButtons;            // Number of buttons for the mouse
        public uint SampleRate;                 // Number of data points per second.
        public bool HasHorizontalWheel;         // True is mouse has wheel for horizontal scrolling else false.
        // ReSharper restore MemberCanBePrivate.Global
        public override string ToString()
        {
            return $"MouseInfo\n Id: {Id}\n NumberOfButtons: {NumberOfButtons}\n SampleRate: {SampleRate}\n HorizontalWheel: {HasHorizontalWheel}\n";
        }
    }
}
