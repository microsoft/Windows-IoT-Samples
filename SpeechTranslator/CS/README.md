# Build the Speech Translator Project

### Component Lists
- An IoT Device (e.g. Upboard)
- A Headset [e.g. the Microsoft LifeChat-3000 Headset](http://www.microsoft.com/hardware/en-us/p/lifechat-lx-3000/JUG-00013) 
- A Mouse
- A router connected to the Internet 
- An ethernet cable
- An HDMI montor and cables 
- A micro-SD card and reader

### Setup your Development PC (Required for Windows IoT Builds 15063 or greater.)
1. Install the Windows ADK for Windows 10, version from [here](https://developer.microsoft.com/en-us/windows/hardware/windows-assessment-deployment-kit)
    - Ensure that "Imaging And Configuration Designer" (ICD) is selected.
    - Install to the default location.    
2. Install the Windows IoT Enterprise ADK Add-Ons from [here](https://developer.microsoft.com/en-us/windows/hardware/windows-assessment-deployment-kit)
    - Install to the default location.
  
### Setup your IoT Device
1. Install a clean O/S to your IoT Device.
2. Connect your device to the router, and connect the router to the Internet.
3. Connect the headset and mouse to your device.
4. Boot your device.
5. Select a language and, if Wi-Fi is supported on your IoT Device, skip the Wi-Fi Configuration step.
6. Provide Voice Permission from Cortana when prompted.  **(Required for Windows 10 IoT Builds 15063 or greater)**
    1. At the first Cortana Prompt, **press OK**.
    2. At the second Cortana Prompt, press **Maybe Later**.     
7. Rename your device
    1. Use Powershell to connect to your device
    2. Change the name of your device to "speechtranslator"
        - **setcomputername speechtranslator**
    3. Reboot your device.
8. Copy a Speech Language Package to your device.  **(Required for Windows 10 IoT Builds 15063 or greater)**
    1. Open a new powershell window on your desktop
    2. From desktop map your device's disk to a local drive:
        - **net use t: \\speechtranslator\c$ /user:\administrator p@ssw0rd)**
    3. Copy a language package from your desktop to your device.  
        *Note: Language packages are available in this folder:* c:\Program Files (x86)\Windows Kits\10\MSPackages\retail\ **your-processor-type** \fre
        
        For example, to copy Spanish Mexican to an arm based device:
        - **copy "c:\Program Files (x86)\Windows Kits\10\MSPackages\retail\amd64\fre\Microsoft-Windows-OneCore-Microsoft-SpeechData-es-MX-Package.cab" t:**                   
    4. Unmap your local drive (e.g. net use /delete t:).
    
9. Apply the Speech Language Package to your device.  **(Required for Windows 10 IoT Builds 15063 or greater)**
    
    *Note: Your device should reboot after committing each update and a "spinning gears" screen will appear until the update completes.*
    
    - **applyupdate -stage c:\Microsoft-Windows-OneCore-Microsoft-SpeechData-es-MX-Package.cab**
    - **applyupdate -commit**
 
10. Wait for your device to reboot

### Setup Azure with Cognitive Services
1. Configure [these Service](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate) to your Azure account with the Cognitive Services APIs.
2. After creating your account and subscribing to the Cogntive Services APIs make note of one of the subscription keys for your account.
    From Azure Web Portal Select:
    - "All Resources" (may appear as a 3x3 grid icon).
    - Select your CognitiveServices subscription from the list.
    - Under the "Cognitive Services account" menu, select "Keys". 
    - Make note of *either* Key 1 or Key 2, you will need to add this key to the sample source before rebuilding.
    
### Setup your sample
1. Download the sample from Windows IoT Enterprise to your local PC.
2. Open the solution file in visual studio.
3. Open the constantParam.cs file.
4. Replace the subscriptionKey with either Key 1 or Key 2 [from instructions above](#Setup-Azure-with-Cognitive-Services).
5. Rebuild the solution.

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
  
 Click the Start button to search for the app by name, and then launch it.

* Deploy and run on your device.
* While wearing your headset, Press "Start Talk".
* Say something in English, the "Message Recognized" box should contain the spoken English phrase.
* Press "Stop Talk".
