# HID Injection

Windows 10 IoT Enterprise HID Injector sample will allow you to deploy a driver to perform low level injection of touch, keyboard and mouse events, and can be used until the SendInput equivelent API is available.

The HID Injection sample leverages the [Virtual HID Framework](https://msdn.microsoft.com/en-us/library/windows/hardware/dn925056(v=vs.85).aspx). 

# Prerequisites 
In order to build this driver you will need the following:

  * [Latest Visual Studio 2019](https://visualstudio.microsoft.com/downloads/).
  * [Windows 10 Driver Development Kit](https://docs.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk).
  * [Windows 10 SDK](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk/).
  * Download the [ms-iot Samples repository](https://github.com/ms-iot/samples/archive/develop.zip) from GitHub, then expand it.

Optional:

  * If you have git or the github app installed, you can clone the repository as well.

## Building and Deploying the HID Injector
  1. Be sure you've installed the Visual Studio update and the Windows 10 Driver Development kit before continuing.
  2. On your development machine, use Windows Explorer to navigate to the folder where you downloaded or cloned samples.
  3. Open the project located ```Samples\HidInjector\HidInjector.sln```.
  4. Select the architecture you intend to deploy to: x64 for UP-BOARD.
  5. You can now build the solution.

### Copying the HID Injector to your device
  1. Under the driver Directory you will find the inf and sys file, Copy that folder and send it to IoT Enterprise device
  2. Copy the testvhid application file from release folder.
  3. Send this file to the IoT Enterprise device.

### Installing the HID Injector
   1. Install the inf file in the Windows IoT Enterprise device.
   2. You will see the dialogue box of "The operation completed successful".
   3. Run the "Testvhid.exe" file in command prompt under Administrator mode.
   4. You will see the message ".... sending control to the device". without any error message.
   5. The HID injector device installed in IoT device.

### Verify installation
If you've installed the driver, verify the install by navigating to the Device Manager and looking for the HID Injection Sample node.

## Using the HID Injector
Included in the solution is a C++ console application used to demonstrate communication with the Hid injection Driver. The Driver is discovered by class using "CM_Get_Device_Interface_List". 
The sample application will inject Touch, Keyboard and Mouse events by synthesizing a HID block, and calling the driver with that block.
