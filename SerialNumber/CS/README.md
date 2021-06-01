---
page_type: sample
urlFragment: serial-number
languages: 
  - csharp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: Read the device serial number for a Windows 10 IoT Core device.
---

# Serial Number

This sample demonstrates how to get the device serial number as documented here: [Access SMBIOS information from a Universal Windows App](https://docs.microsoft.com/en-us/windows/desktop/SysInfo/access-smbios-information-from-a-universal-windows-app).

In order for this to work the restricted capability must be declared in the appxmanifest, and the minimum target version for the project must be 17134 or higher.
   
## Step 1: Load the project in Visual Studio  

1. Make sure you have a local copy of the Windows IoT Samples. (Download a zip of this repository, and make a copy of the folder on your disk). 

1. Open this sample (Serial Number) in Visual Studio 2019. This is a Universal Windows application. 

## Step 2: Deployment

1. Select your architecture. 
> [!NOTE]
>
> If you're building for the UP Board, select `x64` as the architecture.   
  
1. Select **Local Machine** to point to IoT device

1. Press **F5** on your keyboard to deploy to your device. 

## Step 3: [Generate an App Package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)
  
## Step 4: [Install Your App Package using an Install Script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)
  
## Step 5: Launch the application 

## Serial Number Sample Code

```
namespace serialnumber
{
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            try
            {
                SerialNumber.Text = Windows.System.Profile.SystemManufacturers.SmbiosInformation.SerialNumber;
            }
            catch (Exception ex)
            {
                SerialNumber.Text = ex.Message;
            }
        }

        private void GetSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SerialNumber.Text = Windows.System.Profile.SystemManufacturers.SmbiosInformation.SerialNumber;
            }
            catch (Exception ex)
            {
                SerialNumber.Text = ex.Message;
            }
        }
    }
}
```
