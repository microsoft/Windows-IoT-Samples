---
page_type: sample
urlFragment: RFIDForIoT
languages:
  - cpp
products:
  - windows
  - windows-iot
  - windows-10-iot-Enterprise
description: "This sample shows how to read the RFID Tag from RFID Scanner in your Windows 10 IoT Enterprise device."
---

# RFID scanner with Windows 10 IoT Enterprise

In this sample, we will demonstrate how to read the RFID Tag from MFRC522 Scanner and Beep the Buzzer when the card Scans.
Keep in mind that the GPIO APIs are only available on Windows 10 IoT Enterprise, so this sample cannot run on your desktop.

## Load the project in Visual Studio

## Connect the MFRC522 to your Windows 10 IoT Enterprise device

You'll need a few components:

* MFRC522 scanner
* a Piezo Buzzer (If you want a beep when a card scans)
* a breadboard and a couple of connetor wires

### For Upboard (Upboard)

1. Connect RFID SDA to Pin 24
2. Connect RFID SCK to Pin 23
3. Connect RFID MOSI to Pin 19
4. Connect RFID MISO to Pin 21
5. Connect RFID GND to Pin 6
6. Connect RFID RST to Pin 38
7. Connect RFID 3.3V to Pin 1 (For some higher frequency cards this might need 5V)
8. Connect Piezo Buzzer '+' to Pin 32
9. Connect Piezzo Buzzer '-' to Pin 6

For reference, here is the pinout of the Upboard:

![](../Resources/Upboard_Pinout.png)

## Deploy your app

* If you are using  Upboard Choose `Release` and `x64` configuration.
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
 
(1)  After power on the Upboard, Press Del or F7 to enter the BIOS setting.
 
(2)  Under the "Boot -> OS Image ID" Tab:
     Select "Windows 10 IoT Core".
 
(3)  Under the "Advance" Tab:
     Select "Hat Configuration", make "LPSS SPISupport" as "Enabled" then Click on "GPIO Configuration in Pin Order".

(4)  Configure the Pins you are using in the sample as "INPUT" or "OUTPUT".

    In this sample make PIN 32 as "OUTPUT" and initial value as "LOW".

If you need guidance click Link: [here](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

 Click the Start button to search for the app by name, and then launch it.

Scan the 13.56Mhz sample card that comes with the scanner. You should be able to see a card ID pop up on the screen.

Congratulations! You just read an ID off of a RFID card.

## Let’s look at the code

This sample app relies on MFRC522 library written by a github user Michiel Lowijs.
The original library can be found here [MFRC522](https://github.com/mlowijs/mfrc522-netmf).
We have adapted this library to Universal Windows platform. The adapted library can be found in the project directory by the name Mfrc522Lib.
Along with the Mfrc522Lib, the samples directory also contains a PeizoBuzzerLib if you want to use the Piezo Buzzer with the sample.

	
## Additional resources
* [Documentation for all samples](https://developer.microsoft.com/en-us/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.

## Note

Make sure that LowLevel Capabilities in set in PackageAppManifest.
* To do that go to Package.appxmanifesto and view the code
* Under Capabilities if you can find "DeviceCapability Name="lowLevel"/" then your lowLevel Capabilities is enabled.
* If this line "DeviceCapability Name="lowLevel"/" is not present then add it to enable the LowLevel mode and save the PackageAppManifest.
