---
page_type: sample
urlFragment: hello-blinky
languages: 
  - csharp
  - cpp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: A sample that shows how to make an LED attached to a GPIO pin blink on and off for Windows 10 IoT Enterprise.
---

# “Hello, blinky!”

We will create a simple LED blinking app and connect a LED to your Windows 10 IoT Enterprise device.

> [!NOTE]
>
> GPIO APIs are not available on Desktop - this sample will be based on the Up Board. 

## Step 1: Load the project in Visual Studio 2019

* * *

1. Open the application in [Visual Studio 2019](https://www.visualstudio.com/downloads/)
2. Set the architecture in the toolbar dropdown. If you’re building for Up Board, select `x64`.

## Step 2: Connect the LED to your Windows IoT device

* * *

You’ll need a few components:

*   a LED (any color you like)

*   a 220 Ω resistor for the Up Board

*   a breadboard and a couple of connector wires

![Electrical Components](../../Resources/components.png)

### For Up Board

1.  Connect the shorter leg of the LED to GPIO 5 (pin 15 on the expansion header) on the Up Board.
2.  Connect the longer leg of the LED to the resistor.
3.  Connect the other end of the resistor to one of the 3.3V pins on the Up Board.
4.  Note that the polarity of the LED is important. (This configuration is commonly known as Active Low)

And here is the pinout of the Up Board:

![](../../Resources/UpBoard_Pinout.png)

Here is an example of what your breadboard might look like with the circuit assembled:

![](../../Resources/breadboard_assembled_UpBoard_kit.png)

## Step 3: Deploy your app

* * *

1. [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)
2. [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)
3. If you are using UPBOARD, you have to setup the BIOS UART configuration.
	1. Once you power on the UP board, select the **Del** or **F7** key on your keyboard to enter the BIOS setting.
	
	1. Navigate to **Boot** > **OS Image ID** tab, and select **Windows 10 IoT Core**.
	
  	1. Navigate to the **Advance** tab and select the **Hat Configuration** and select **GPIO Configuration in Pin Order**.

  	1. Configure the Pins you are using in the sample as **INPUT** or **OUTPUT**.

  	1. For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

4. Click the Start button to search for the app by name, and then launch it.

5. The Blinky app will deploy and start on the Windows IoT device, and you should see the LED blink in sync with the simulation on the screen.

![](../../Resources/blinky-screenshot.png)

	@@ -192,6 +180,7 @@ Remember that we connected the other end of the LED to the 3.3 Volts power suppl

## Additional resources

* [Windows 10 IoT Enterprise Documentation](https://docs.microsoft.com/windows/iot/iot-enterprise/getting_started)
* [Documentation for all samples](https://developer.microsoft.com/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.
