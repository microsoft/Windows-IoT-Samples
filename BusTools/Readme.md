---
page_type: sample
urlFragment: bustools
languages:
  - cpp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: IoT Bus tools to interact with GPIO, I2C, PWM, SPI and UART 
---

# Windows IoT Bus Tools

This folder contains tools that let you interact with GPIO, I2C, PWM, SPI, and UART on the command line. 

They will run on any edition of Windows, including [Windows 10 IoT Enterprise](https://docs.microsoft.com/windows/iot/iot-enterprise/getting_started).

The tools are:

* GPIOTestTool
* I2CTestTool
* PWMTestTool
* SPITestTool
* MinComm (UART)

## Steps to Build the Project
1. Download [Visual Studio 2019](https://www.visualstudio.com/downloads/).

1. Select packages for C++, Windows UWP, and Windows Desktop app development.

1. Download the latest version of the [Windows SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk).

1. From the start menu, run Developer Command Prompt for VS 2019

1. Navigate to the root of this repository and run:

      ```msbuild /p:Platform=x64 /p:Configuration=Release```

Valid values of Platform are: `ARM`, `ARM64`, `x86`, `x64`

Valid values of Configuration are: Release, Debug

## BIOS Settings for the UP Board
>[!NOTE]
>
> If you are using the [UP Board](https://up-board.org/up/specifications/), you will have to set up the BIOS GPIO configuration.

1. Configure the BIOS GPIO on the UP Board:

1. Once you power on the UP board, select the **Del** or **F7** key on your keyboard to enter the BIOS setting.

1. Navigate to **Boot** > **OS Image ID** tab, and select **Windows 10 IoT Core**.

1. Navigate to the **Advance** tab and select the **Hat Configuration** and select **GPIO Configuration in Pin Order**.

    1. For the PWMTestTool: select the **Hat Configuration** and select **LPSS PWM #1/#2 Support --> ACPI**.

1. Configure the Pins you are using in the sample as **INPUT** or **OUTPUT**.

For more information, please review the [UP Board Firmware Settings.](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf)
