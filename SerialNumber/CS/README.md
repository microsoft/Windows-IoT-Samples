
# Serialnumber app 

Read the device serial number for a Windows 10 IoT Enterprise device. 
  
### Load the project in Visual Studio  
  
You can find the source code for this sample by downloading a zip of all of our samples. Make a copy of the folder on your disk and open the project from Visual Studio 2019.  
  
This is a Universal Windows application 

### Deploy your app  
  
If you're building for UPBoard, select `x64` as the architecture.   
  
Select **Local Machine** to point to IoT device and hit F5 to deploy to your device. 

### Generate an app package

Steps to follow :

 In Solution Explorer, open the solution for your application project.
 Right-click the project and choose Publish->Create App Packages (before Visual Studio 2019 version 16.3, the Publish menu is named Store).
 Select Sideloading in the first page of the wizard and then click Next.
 On the Select signing method page, select whether to skip packaging signing or select a certificate for signing. You can select a certificate from your local certificate store, select a certificate file, or create a new certificate. For an MSIX package to be installed on an end user's machine, it must be signed with a cert that is trusted on the machine.
 Complete the Select and configure packages page as described in the Create your app package upload file using Visual Studio section.

 If you need guidance click Link: [here].(https://docs.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#generate-an-app-package)  
  
### Install your app package using an install script

Steps to follow :
 Open the *_Test folder.
 Right-click on the Add-AppDevPackage.ps1 file. Choose Run with PowerShell and follow the prompts.
 When the app package has been installed, the PowerShell window displays this message: Your app was successfully installed.

 If you need guidance click Link: [here].(https://docs.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)  
  
 Click the Start button to search for the app by name, and then launch it.

### SerialNumber Sample Code

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

*This sample demonstrates how to get the device serial number as documented here: [Access SMBIOS information from a Universal Windows App](https://docs.microsoft.com/en-us/windows/desktop/SysInfo/access-smbios-information-from-a-universal-windows-app).

*In order for this to work the restricted capability must be declared in the appxmanifest, and the minimum target version for the project must be 17134 or higher.

