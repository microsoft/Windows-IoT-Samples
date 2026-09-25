---
page_type: sample
urlFragment: IoTHubClients
languages:
  - csharp
  - cpp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: This sample demonstrates how to connect to IoT Hub, send telemetry, and monitor and respond to device twin state changes.
---

# Azure IoT Hub Client Sample

This sample demonstrates how to connect to IoT Hub, send telemetry, and monitor and respond to device twin state changes.

The sample is Microsoft IoT Central compatible. See below for a complete walk-through.

# Walk-through: Connecting to Microsoft IoT Central

This article describes how, as a device developer, to connect a device running a Windows 10 IoT Enterprise device to your Microsoft IoT Central application using the C# programming language.

### Before you begin

To complete the steps in this article, you need the following:

- A Microsoft IoT Central application created from the Sample Devkits application template. For more information, see [Create your Microsoft IoT Central Application](https://docs.microsoft.com/en-us/microsoft-iot-central/howto-create-application).
- A device running the [Windows 10 IoT Enterprise OS](https://docs.microsoft.com/windows/iot/iot-enterprise/downloads).  For this walkthrough, we will use an UP Board.  
- [Visual Studio 2019](https://www.visualstudio.com/downloads/) installed (only needed if you are going to build/deploy the source code). 
  - With 'The Universal Windows Platform development' workload installed.

## Step 1: Add a Real Device in Microsoft IoT Central

In Microsoft IoT Central, 

- Make a note of the device connection string. For more information, see Add a real device to your [Microsoft IoT Central application](https://docs.microsoft.com/en-us/microsoft-iot-central/tutorial-add-device).

## Step 2: Setup A Physical Device

To setup a physical device, we need:

- A device running Windows IoT Enterprise operating system.
  - To do that, follow the steps described [here](https://developer.microsoft.com/en-us/windows/iot/getstarted/prototype/setupdevice).
- A client application that can communicate with Microsoft IoT Central.
  - You can either build your own custom application using the Azure SDK and deploy it to your device (using Visual Studio). OR
  - You can download a pre-built sample and simply deploy and run it on the device.

## Step 3: Deploy The Pre-built Sample Client Application to The Device

To deploy the client application to your Windows IoT Device,

- Ensure the connection string is stored on the device for the client application to use.
  - On the desktop, save the connection string in a text file named connection.string.iothub.
  - Copy the text file to the device’s document folder:
     - <i>[device-IP-address]</i>\C$\Data\Users\DefaultAccount\Documents\connection.string.iothub
     
## Step 4: [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)
  
## Step 5: [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)

The application should launch on the device, and will look something like this:

<img src="../../Resources/images/Azure/IoTHubClients/IoTHubClientScreenshot.png">

In Microsoft IoT Central, you can see how the code running on the Up-Board interacts with the application:

- On the Measurements page for your real device, you can see the telemetry.
- On the Properties page, you can see the value of the reported Die Number property.

## Source Code

You can find the source code for this sample by downloading a zip of all of our samples.

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.

