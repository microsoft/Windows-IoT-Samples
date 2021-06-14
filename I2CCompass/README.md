---
page_type: sample
urlFragment: I2CCompass
languages:
  - csharp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: This sample uses I2C on Windows 10 IoT Enterprise to communicate with an HMC5883L Magnetometer device.
---

# I2C Compass

This sample uses I2C on Windows IoT Enterprise to communicate with an HMC5883L Magnetometer device.

## Step 1: Set up your hardware
Please reference the datasheet for the HMC5883L found [here](https://github.com/microsoft/Windows-iotcore-samples/blob/develop/Samples/I2CCompass/HMC5883L_3-Axis_Digital_Compass_IC.pdf).

For more information on compass heading using magnetometers please see [here](https://github.com/microsoft/Windows-iotcore-samples/blob/develop/Samples/I2CCompass/AN203_Compass_Heading_Using_Magnetometers.pdf).

## Step 2: Load the project in Visual Studio
You can find the source code for this sample by downloading a zip of all of our samples. Extract the zip to your disk, then open the `Samples\I2CCompass\CS\I2CCompass.sln` project from Visual Studio.

## Step 3: Deploy the sample

* Choose `Release` and `x64` configuration.
* Compile the Solution file


### [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)

### [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)

### BIOS Settings for UP Board
If you are using an UP Board, you have to setup the BIOS SPI configuration.

1. Once you power on the UP board, select the **Del** or **F7** key on your keyboard to enter the BIOS setting.

1. Navigate to **Boot** > **OS Image ID** tab, and select **Windows 10 IoT Core**.

1. Navigate to the **Advance** tab and select the **Hat Configuration** and set **I2C0/GPIO SELECTION** as **I2C0**.

1. For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

1. Click the Start button to search for the app by name, and then launch it.


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
