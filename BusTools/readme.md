---
page_type: sample
urlFragment: bustools
languages:
  - cpp
products:
  - windows 10
  - windows-iot
  - windows-10-iot-Enterprise
description: "IoT Bus tools to interact with Gpio, I2c, Pwm, Spi and UART."
---

# Windows IoT Bus Tools

This folder contains tools that let you interact with Gpio, I2c, Pwm, Spi, and UART on the command line. They will run on any edition of Windows, including Windows IoT Core and Windows 10 IoT Enterprise. The tools are:

- GpioTestTool
- I2cTestTool
- PwmTestTool
- SpiTestTool
- MinComm (UART)

## Building

1. Download [Visual Studio 2019](https://www.visualstudio.com/downloads/). Select options for C++, Windows UWP, and Windows Desktop app development. You may need to download the latest version of the [Windows SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk).
1. From the start menu, run `Developer Command Prompt for VS 2019`
1. Navigate to the root of this repository and run:

```powershell
msbuild /p:Platform=x64 /p:Configuration=Release
```

Valid values of Platform are: `ARM, ARM64, x86, x64`

Valid values of Configuration are: `Release, Debug`

### BIOS Settings for the UP Board

[!NOTE]

If you are using the [Up Board](https://up-board.org/up/specifications/), you will have to set up the BIOS I2C configuration.


### BIOS Settings for UPBOARD

Steps to follow:
 
(1)	After power on the Upboard, Press Del or F7 to enter the BIOS setting.
 
(2)	Under the "Boot -> OS Image ID" Tab:
    Select "Windows 10 IoT Core".
 
(3) Under the "Advance" Tab: Select "Hat Configuration" and make "I2C0/GPIO SELECTION" as "I2C0".

If you need guidance click Link: [here](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

