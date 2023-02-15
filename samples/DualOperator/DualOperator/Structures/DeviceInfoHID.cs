namespace DualOperator.Structures
{
    public struct DeviceInfoHID
    {
        public uint VendorID;       // Vendor identifier for the HID
        public uint ProductID;      // Product identifier for the HID
        public uint VersionNumber;  // Version number for the device
        public ushort UsagePage;    // Top-level collection Usage page for the device
        public ushort Usage;        // Top-level collection Usage for the device

        public override string ToString()
        {
            return $"HIDInfo\n VendorID: {VendorID}\n ProductID: {ProductID}\n VersionNumber: {VersionNumber}\n UsagePage: {UsagePage}\n Usage: {Usage}\n";
        }
    }
}
