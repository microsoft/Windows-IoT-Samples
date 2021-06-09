---
page_type: sample
urlFragment: I2CCompass
languages:
  - csharp
products:
  - windows
  - windows-iot
  - windows-10-iot-Enterprise
description: This sample uses I2C on Windows 10 IoT Enterprise to communicate with an HMC5883L Magnetometer device.
---

# I2C Compass

This sample uses I2C on Windows IoT Core to communicate with an HMC5883L Magnetometer device.

## Set up your hardware
Please reference the datasheet for the HMC5883L found [here](https://github.com/microsoft/Windows-iotcore-samples/blob/develop/Samples/I2CCompass/HMC5883L_3-Axis_Digital_Compass_IC.pdf).

For more information on compass heading using magnetometers please see [here](https://github.com/microsoft/Windows-iotcore-samples/blob/develop/Samples/I2CCompass/AN203_Compass_Heading_Using_Magnetometers.pdf).

## Load the project in Visual Studio
You can find the source code for this sample by downloading a zip of all of our samples. Extract the zip to your disk, then open the `Samples\I2CCompass\CS\I2CCompass.sln` project from Visual Studio.

## Deploy the sample

* Choose `Release` and `x64` configuration.
* Compile the Solution file

### Generate an app package

Steps to follow :

 * In Solution Explorer, open the solution for your application project.
 * Right-click the project and choose Publish->Create App Packages (before Visual Studio 2019 version 16.3, the Publish menu is named Store).
 * Select Sideloading in the first page of the wizard and then click Next.
 * On the Select signing method page, select whether to skip packaging signing or select a certificate for signing. You can select a certificate from your local certificate store, select a certificate file, or create a new certificate. For an MSIX package to be installed on an end user's machine, it must be signed with a cert that is trusted on the machine.
 * Complete the Select and configure packages page as described in the Create your app package upload file using Visual Studio section.

 If you need guidance click Link: [here](https://docs.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#generate-an-app-package).  
  
### Install your app package using an install script

Steps to follow :
 * Open the *_Test folder.
 * Right-click on the Add-AppDevPackage.ps1 file. Choose Run with PowerShell and follow the prompts.
 * When the app package has been installed, the PowerShell window displays this message: Your app was successfully installed.

 If you need guidance click Link: [here](https://docs.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script).  
 

 If you are using UPBOARD, you have to setup the BIOS SPI configuration.

### BIOS Settings for UPBOARD

Steps to follow:
 
(1)	After power on the Upboard, Press Del or F7 to enter the BIOS setting.
 
(2)	Under the "Boot -> OS Image ID" Tab:
    Select "Windows 10 IoT Core".
 
(3) Under the "Advance" Tab: Select "Hat Configuration" and make "I2C0/GPIO SELECTION" as "I2C0".

If you need guidance click Link: [here](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

## Additional information
The HMC3883L device is accessed through the [Windows.Devices.I2c API](https://docs.microsoft.com/en-us/uwp/api/windows.devices.i2c) using the default I2C controller and the address 0x1E.
```CS
    // Setup the settings to address 0x1E with a 400KHz bus speed
    var i2cSettings = new I2cConnectionSettings(ADDRESS);
    i2cSettings.BusSpeed = I2cBusSpeed.FastMode;

    // Create an I2cDevice with our selected bus controller ID and I2C settings
    var controller = await I2cController.GetDefaultAsync();
    _i2cController = controller.GetDevice(i2cSettings);
```