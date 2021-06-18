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

This folder contains tools that let you interact with GPIO, I2C, PWM, SPI, and UART on the command line. They will run on any edition of Windows, including [Windows 10 IoT Enterprise](https://docs.microsoft.com/windows/iot/iot-enterprise/getting_started) and [Windows 10 IoT Core](https://docs.microsoft.com/windows/iot-core/windows-iot-core).

The tools are:
- GpioTestTool
- I2cTestTool
- PwmTestTool
- SpiTestTool
- MinComm (UART)

## Steps to Build the Project
1. Download [Visual Studio 2019](https://www.visualstudio.com/downloads/).

1. Select packages for C++, Windows UWP, and Windows Desktop app development.

1. Download the latest version of the [Windows SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk).

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

Steps to follow:
 
(1)	After power on the Upboard, Press **Del** or **F7** to enter the BIOS setting.
 
(2)	Under the **Boot -> OS Image ID** Tab:
    Select **Windows 10 IoT Core**.

**For GPIOTestTool**
 
* Navigate to the **Advance** Tab: Select the **Hat Configuration** and select **GPIO Configuration in Pin Order.**

* Configure the Pins you are using in the sample as **INPUT** or **OUTPUT.**

**For I2CTestTool**
 
* Navigate to the **Advance** Tab: Select the **Hat Configuration** and under **LPSS I2C #1Support** Select **ACPI.**
* Configure **I2C0/GPIO SELECTION** as **I2C0.**

**For PWMTestTool**

* Navigate to the **Advance** Tab: Select the **Hat Configuration** and under **LPSS PWM #1Support** Select **ACPI.**
* Configure the Pins you are using in the sample as **INPUT** or **OUTPUT.**

**For SPITestTool**

* Navigate to the **Advance** Tab: Select the **Hat Configuration** and under **LPSS SPISupport** Select **ACPI.**

**For MinComm(UART)**

* Navigate to the **Advance** Tab: Select the **Hat Configuration** and under **LPSS HSUARTSupport** Select **ACPI.**

For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).
