---
page_type: sample
urlFragment: virtual-microphone-array-driver
languages:
  - cpp
products:
  - Windows 10
  - Windows IoT 
  - Windows 10 IoT Enterprise 
description: An app service to blink an LED and a client app to call it with Windows 10 IoT Core.
---

# Virtual Audio Microphone Array

If you are not very familiar with Windows universal drivers yet, we strongly encourage you to start by checking out the following video:

* [Building Universal Drivers - Channel 9 Video](https://channel9.msdn.com/Blogs/WinHEC/Building-a-Universal-Driver)

## Set Up

* Download [Visual Studio 2019](https://www.visualstudio.com/downloads/)

* Install [Windows Driver Kit (WDK)](https://developer.microsoft.com/en-us/windows/hardware/windows-driver-kit). 

## Overview
We will now be walking you through the process of creating and installing a device driver that will run on any Windows 10 IoT Enterprise device.  This driver will be specifically built to be a universal driver.

## Description
The name of the driver in this sample is `virtualaudiomicarray`.  

## Source Code And Binaries
You can find the source code for this sample by downloading a zip of all of our sample folder, and navigating to the `samples\VirtualMicrophoneArrayDriver`.  Make a copy of the folder on your disk and open the solution file from Visual Studio.

Originally derived from the [SYSVAD driver sample](https://github.com/Microsoft/Windows-driver-samples/tree/master/audio/sysvad), this sample illustrates how to construct a virtual microphone array at runtime using monoaural audio devices present on the system.

Before running the sample, the INF must be updated to setup registry keys under the drivers 'Parameters' subkey to identify which audio endpoints should be used as inputs and the format (NumChannels - for the entire array, SamplesPerSecond, BitsPerSample) of each input stream.

Limitations in the sample:
* All input streams must use the same format - this sample performs no format conversion.
* There is no mechanism to control the gain of the input streams.
* The metadata for the microphone array geometry is hardcoded
* CMicArrayMiniportTopology::PropertyHandlerMicArrayGeometry should be updated in micarraytopo.cpp to reflect the actual physical characteristics of the microphone array.
