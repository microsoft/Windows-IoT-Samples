---
page_type: sample
urlFragment: speech-translator
languages:
  - csharp
products:
  - Windows
  - Windows 10 
  - Windows 10 IoT Enterprise
description: Translate your speech to different languages.
---

# Build the Speech Translator Project

## Component Lists
- A Windows 10 IoT Enterprise Device (e.g. UP Board)
- A headset [e.g. EPOS/Sennheiser SC 75 USB Headset](https://www.microsoft.com/en-us/d/epos-sennheiser-sc-75-usb-headset-for-ms-teams/8xqc82x6r516?activetab=pivot%3aoverviewtab) 
- A mouse
- A router connected to the Internet 
- An Ethernet cable or WiFi Adapter
- An HDMI monitor and cables 

## Step 1: Setup your Development PC (Required for Windows IoT Builds 15063 or greater.)
1. Install the Windows ADK for Windows 10, version from [here](https://developer.microsoft.com/en-us/windows/hardware/windows-assessment-deployment-kit)
    - Ensure that "Imaging And Configuration Designer" (ICD) is selected.
    - Install to the default location.    
2. Install the Windows IoT Enterprise ADK Add-Ons from [here](https://developer.microsoft.com/en-us/windows/hardware/windows-assessment-deployment-kit)
    - Install to the default location.
  
## Step 2: Setup your IoT Device
1. Install [Windows 10 IoT Enterprise](https://docs.microsoft.com/windows/iot/iot-enterprise/downloads) to your device 
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
    
    - **applyupdate -stage c:\Microsoft-Windows-OneCore-Microsoft-SpeechData-es-MX-Package.cab**
    - **applyupdate -commit**
 
10. Wait for your device to reboot

## Step 3: Setup Azure with Cognitive Services
1. Configure [these services](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate) to your Azure account with the Cognitive Services APIs.
2. After creating your account and subscribing to the Cognitive Services APIs make note of one of the subscription keys for your account.
    From Azure Web Portal Select:
    - **All Resources**
    - Select your CognitiveServices subscription from the list.
    - Under the **Cognitive Services account** menu, select **Keys**. 
    - Make note of *either* Key 1 or Key 2, you will need to add this key to the sample source before rebuilding.
    
## Step 4: Setup your sample
1. Download this sample and open the solution file in Visual Studio.
1. Open the constantParam.cs file.
1. Replace the subscriptionKey with either Key 1 or Key 2 [from instructions above](#Setup-Azure-with-Cognitive-Services).
1. Rebuild the solution.
1. [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)
1. [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)

## Step 5: Deploy and run on your device
1. While wearing your headset, Press **Start Talk**.
1. Say something in English, the **Message Recognized** box should contain the spoken English phrase.
1. Press **Stop Talk**.
