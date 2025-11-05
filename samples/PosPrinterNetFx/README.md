# PosPrinterNetFx

## What this solution is
`PosPrinterNetFx` is a minimal WPF (.NET Framework 4.8.1) sample that demonstrates how to:
- Discover installed POS printers via Microsoft POS for .NET (`PosExplorer`).
- Allow the user to select a printer at runtime.
- Enter custom receipt text.
- Print that text using a `PosPrinter` service object.

It is intended as a starting point for integrating OPOS / POS for .NET receipt printers in a desktop application.

It has been tested with the Epson TM-T88V and XPrinter XP-80 POS printers. 

POS for .NET only supports .NET Framework at this time. It does not support .NET Core/.NET. 

Note: To use POS for .NET on an Arm64 platform, you must use emulation (i.e. Build the application targeting x64 or x86 and run the application as is).

## Pre-requisite software
Before building and running, ensure the following are installed and configured:

- Windows 10/11 with .NET Framework 4.8.1 (Developer Pack recommended).
- Microsoft POS for .NET 1.14.1 (or the version that matches your device vendor requirements). This installs the POS for .NET runtime and configuration utilities.
- Manufacturer OPOS / POS for .NET Service Object (SO) packages for each printer model you intend to use. These typically include:
  - Service Object DLLs.
  - Registry entries or XML configuration files.
  - A setup/config utility.

## Setting up the Service Objects and configuring printers
Service Objects (SOs) are the vendor-provided components that implement device-specific logic behind the POS for .NET interfaces. You must install and configure them before this sample will detect and use the printers.

## Step-by-step
1. Install Microsoft POS for .NET:
   - Download Microsoft Point of Service for .NET v1.14.1 (POS for .NET) from Microsoft: 
   - Run setup. Confirm `PosExplorer` assemblies are placed (usually in `C:\Program Files (x86)\Microsoft Point Of Service\`).

2. Install vendor Service Object:
   - Run the POS printer vendor's OPOS or POS for .NET installer
   - This registers the printer service objects with Windows.

3. Use the vendor configuration utility:
   - Launch the vendor's configuration tool.
   - Add or register each physical printer. Assign a logical device name.
   - Test the printer from the tool (health check / sample print) to confirm communication.

4. Build and run this solution:
   - Open the solution in Visual Studio.
   - Restore any missing references (ensure `Microsoft.PointOfService` is referenced).
   - Run. The ComboBox will list all devices where `device.Type == DeviceType.PosPrinter`.
   - Select a printer (first one is auto-selected by code) and enter receipt text.
   - Click the print button.

## Notes on logical names
- The sample currently displays `ServiceObjectName` values in the ComboBox.
- If you prefer descriptive names, you can switch to `device.Description` or `device.LogicalName` depending on vendor implementation.

## Troubleshooting
- Printer not listed: Confirm the service object is installed and the logical device configured. Restart Windows after installation.
- Claim/open failure: Another application may have the device claimed; close other POS apps.
- Refer to printer vendor's documentation for additional troubleshooting

## Documentation
- Microsoft POS for .NET SDK documentation: (https://learn.microsoft.com/en-us/dotnet/framework/additional-apis/pos-for-net/)
