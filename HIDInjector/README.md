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
  1. Open a network share on your IoT Enterprise device by opening the Run dialog (Win-R), then entering \\```IP for your IoT Enteprise device\c$```. Enter credentials if prompted.
  2. Create a ```deploy``` folder on your IoT Enterprise device. 
  3. In Visual Studio, Right Click on the HidInjectorKd project, then select ```Open Folder in File Explorer```.
  4. In the File Explorer that opened on your project, Navigate to the driver directory.
  5. Now, copy the Microsoft.HidInjectionSample.HidInjectionSample.cab to the network folder you opened in the first step.

### Installing the HID Injector
   1. Use [Powershell](/en-us/win10/samples/PowerShell.htm) to connect to your device. 
   2. Once connected, change to your deployment direcory by typing ```cd deploy```.
   3. Now prepare the install of the driver by typing ```ApplyUpdate -stage Microsoft.HidInjectionSample.HidInjectionSample.cab```.
   4. Now commit the install by typing ```ApplyUpdate -commit```.
   5. Your IoT Enterprise device will reboot, and apply the update.

### Verify installation
If you've installed the driver, verify the install by navigating to the Web management console http://<your device ip>:8080/devicemanager.htm and looking for the HID Injection Sample node.

## Using the HID Injector
Included in the solution is a C++ console application used to demonstrate communication with the Hid injection Driver. The Driver is discovered by class using "CM_Get_Device_Interface_List". 
The sample application will inject Touch, Keyboard and Mouse events by synthesizing a HID block, and calling the driver with that block.
