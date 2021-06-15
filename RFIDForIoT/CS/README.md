# RFID scanner with Windows 10 IoT Enterprise

In this sample, we will demonstrate how to read the RFID Tag from MFRC522 Scanner and beep the buzzer when the card scans.
Keep in mind that the GPIO APIs are only available on Windows 10 IoT Enterprise boards, so this sample cannot run on your desktop.

## Step 1: Load the project in Visual Studio

## Step 2: Connect the MFRC522 to your Windows 10 IoT Enterprise device

You'll need a few components:

* MFRC522 scanner
* a Piezo Buzzer (If you want a beep when a card scans)
* a breadboard and a couple of connector wires

### For UP Board

1. Connect RFID SDA to Pin 24
2. Connect RFID SCK to Pin 23
3. Connect RFID MOSI to Pin 19
4. Connect RFID MISO to Pin 21
5. Connect RFID GND to Pin 6
6. Connect RFID RST to Pin 38
7. Connect RFID 3.3V to Pin 1 (For some higher frequency cards this might need 5V)
8. Connect Piezzo Buzzer '+' to Pin 32
9. Connect Piezzo Buzzer '-' to Pin 6

For reference, here is the pinout of the UP Board:

![](../../Resources/Upboard_Pinout.png)

## Step 3: Deploy your app

* If you are using  UP Board Choose `Release` and `x64` configuration.
* Compile the Solution file

## Step 4: [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)

## Step 5: [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)



## Step 6: BIOS Settings for UP Board

If you are using UP Board, you have to setup the BIOS SPI configuration.

Once you power on the UP board, select the **Del** or **F7** key on your keyboard to enter the BIOS setting.

   1. Navigate to **Boot** > **OS Image ID** tab, and select **Windows 10 IoT Core**.

   1. Navigate to the Advance tab and select the **Hat Configuration**, select **LPSS SPISupport** as **Enabled** and then Click on "GPIO Configuration in Pin Order".

   1. Configure the Pins you are using in the sample as **INPUT** or **OUTPUT**.

   1. In this sample make PIN 32 as "OUTPUT" and initial value as "LOW".

   1. For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

1. Click the **Start** button to search for the app by name, and then launch it.

1. Scan the 13.56Mhz sample card that comes with the scanner. You should be able to see a card ID pop up on the screen.

Congratulations! You just read an ID off of a RFID card.

## Let’s look at the code

This sample app relies on MFRC522 library written by a github user Michiel Lowijs.
The original library can be found here [MFRC522](https://github.com/mlowijs/mfrc522-netmf).
We have adapted this library to Universal Windows platform. The adapted library can be found in the project directory by the name Mfrc522Lib.
Along with the Mfrc522Lib, the samples directory also contains a PeizoBuzzerLib if you want to use the Piezo Buzzer with the sample.

	
## Additional resources
* [Documentation for all samples](https://developer.microsoft.com/en-us/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.

## Additional Notes

Make sure that LowLevel Capabilities in set in PackageAppManifest.
* To do that go to Package.appxmanifesto and view the code
* Under Capabilities if you can find "DeviceCapability Name="lowLevel"/" then your lowLevel Capabilities is enabled.
* If this line "DeviceCapability Name="lowLevel"/" is not present then add it to enable the LowLevel mode and save the PackageAppManifest.
