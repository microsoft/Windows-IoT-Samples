---
page_type: sample
urlFragment: gpio-kmdf-demo
languages:
  - cpp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: Create a simple GPIO demo.
---

# GPIO KMDF Demo

Windows 10 IoT Enterprise GPIO KMDF Demo sample will allow you to deploy a GPIO Kernel driver to IoT Device.

# Prerequisites 
In order to build this driver you will need the following:

  * [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/).
  * [Windows 10 Driver Development Kit](https://docs.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk).
  * [Windows 10 SDK](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk/).

## Building and Deploying the GPIO KMDF
  1. Be sure you've installed the Visual Studio update and the Windows 10 Driver Development kit before continuing.
  2. On your development machine, use Windows Explorer to navigate to the folder where you downloaded or cloned samples.
  3. Open the project located ```Samples\GPIOKMDFDemo\GPIOkmdfdemo.sln```.
  4. Select the architecture you intend to deploy to: x64 for UP-BOARD.
  5. You can now build the solution.

### Copying the GPIOKMDF folder to your device
  1. Under the release folder you will find the .inf and .sys files. 
  2. Copy that folder and send it to IoT Enterprise device

### Installing the GPIO KMDF Drivers
   1. Install the .inf file in the Windows IoT Enterprise device.
   2. You will see the dialogue box of "The operation completed successful".
   3. The GPIO KMDF device drivers installed in IoT device

### Verify installation
If you've installed the driver, verify the install by navigating to the system32 folder under Windows and looking for the GPIO KMDF Demo file.



This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.
