# Using an app service to blink an LED
We’ll create a simple Blinky [app service](https://docs.microsoft.com/en-us/windows/uwp/launch-resume/how-to-create-and-consume-an-app-service) and connect a LED to your Windows IoT Enterprise device (Up-Board). We'll also create a simple app service client that blinks the LED. Be aware that the GPIO APIs are only available on Windows IoT Enterprise, so this sample cannot run on your desktop.

## Set up your hardware
___
The hardware setup for this sample is the same as the Blinky sample.
Note that the app will not run successfully if it cannot find any available GPIO ports.

## Load the projects in Visual Studio
___

* You can find the source code for this sample by downloading a zip of all of our samples in WIndows 10 IoT Enterprise folder and navigating to the `samples-develop\AppServiceBlinky`. 
* Make a copy of the folder on your disk and open the projects from Visual Studio.  
* BlinkyService.sln implements the app service and must be started first.  
* BlinkyClient.sln implements the app service client.
* Verify that the value of connection.PackageFamilyName in StartupTask.cs and MainPage.xaml.cs matches the value output in the output window by BlinkyService.
* If it doesnot any value output, You will get the value by marking the breakpoint in debug mode for #StartupTask.cs at System.Diagnostics.Debug.WriteLine("Service closed. Status=" + args.Status.ToString()).

## Deploy your app

* * *

With the application open in Visual Studio 2019, set the architecture in the toolbar dropdown. If you’re building for UpBaord, select `x64`.

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

 If you are using UPBOARD, you have to setup the BIOS GPIO configuration.

### BIOS Settings for UPBOARD

Steps to follow:
 
(1)	After power on the Upboard, Press Del or F7 to enter the BIOS setting.
 
(2)	Under the "Boot -> OS Image ID" Tab:
    Select "Windows 10 IoT Core".
 
(3)	Under the "Advance" Tab:
    Select "Hat Configuration" and Click on "GPIO Configuration in Pin Order".

(4) Configure the Pins you are using in the sample as "INPUT" or "OUTPUT".

    In this sample make PIN 15 as "OUTPUT" and initial value as "HIGH".

If you need guidance click Link: [here](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).
 
 Click the Start button to search for the app by name, and then launch it.

## Let's look at the code
___
The code is in 2 projects BlinkyService and BlinkyClient.  First we'll look at BlinkyService.

## Adding an app service
___
To add an appservice to our background application first we need to open appxmanifest.xml in a text editor and add an extension with Category="windows.AppService"

```XML
<Extensions>
    <uap:Extension Category="windows.appService" EntryPoint="BlinkyService.StartupTask">
        <uap:AppService Name="BlinkyService" />
    </uap:Extension>
    <Extension Category="windows.backgroundTasks" EntryPoint="BlinkyService.StartupTask">
        <BackgroundTasks>
        <iot:Task Type="startup" />
        </BackgroundTasks>
    </Extension>
</Extensions>
```

Next we'll add a check in the StartupTask::Run method to see if the application is being started as an appservice

```csharp
//Check to determine whether this activation was caused by an incoming app service connection
var appServiceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
if (appServiceTrigger != null)
{
    //Verify that the app service connection is requesting the "BlinkyService" that this class provides
    if (appServiceTrigger.Name.Equals("BlinkyService"))
    {
        //Store the connection and subscribe to the "RequestRecieved" event to be notified when clients send messages
        connection = appServiceTrigger.AppServiceConnection;
        connection.RequestReceived += Connection_RequestReceived;
    }
    else
    {
        deferral.Complete();
    }
}
```

At the beginning of BlinkyService's StartupTask::Run get the deferral object and set up a Canceled event handler to clean up the deferral on exit.

```csharp
deferral = taskInstance.GetDeferral();
taskInstance.Canceled += TaskInstance_Canceled;
```

When the Canceled event handler is called Complete the deferral for this instance of the app service if one exists.  If the deferral is not completed then the app service process will be killed by the operating system even if other clients still have connections open to the app service.

```csharp
private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
{
    if (deferral != null)
    {
        deferral.Complete();
        deferral = null;
    }
}
```

Finally we need to handle service requests:

```csharp
private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
{
    var messageDeferral = args.GetDeferral();

    //The message is provided as a ValueSet (IDictionary<String,Object)
    //The only message this server understands is with the name "requestedPinValue" and values of "Low" and "High"
    ValueSet message = args.Request.Message;
    string requestedPinValue = (string)message["requestedPinValue"];


    if (message.ContainsKey("requestedPinValue"))
    {

        if (requestedPinValue.Equals("High"))
        {
            pin.Write(GpioPinValue.High);
        }
        else if (requestedPinValue.Equals("Low"))
        {
            pin.Write(GpioPinValue.Low);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Reqested pin value is not understood: " + requestedPinValue);
            System.Diagnostics.Debug.WriteLine("Valid values are 'High' and 'Low'");
        }

    }
    else
    {
        System.Diagnostics.Debug.WriteLine("Message not understood");
        System.Diagnostics.Debug.WriteLine("Valid command is: requestedPinValue");
    }

    messageDeferral.Complete();
}
```

## Connect to the app service in BlinkyClient
___
When the client starts it opens a connection to the client.  The string assigned to connection.PackageFamilyName uniquely identifies the service we want to connect to.

```csharp
AppServiceConnection connection;
BackgroundTaskDeferral deferral;
ThreadPoolTimer timer;
string requestedPinValue;

public async void Run(IBackgroundTaskInstance taskInstance)
{
    deferral = taskInstance.GetDeferral();

    //Connect to the "BlinkyService" implemented in the "BlinkyService" solution
    connection = new AppServiceConnection();
    connection.AppServiceName = "BlinkyService";
    connection.PackageFamilyName = "BlinkyService-uwp_2yx4q2bk84nj4";
    AppServiceConnectionStatus status = await connection.OpenAsync();

    if (status != AppServiceConnectionStatus.Success)
    {
        deferral.Complete();
        return;
    }

    //Send a message with the name "requestedPinValue" and the value "High"
    //These work like loosely typed input parameters to a method
    requestedPinValue = "High";
    var message = new ValueSet();
    message["requestedPinValue"] = requestedPinValue;
    AppServiceResponse response = await connection.SendMessageAsync(message);

    //If the message was successful, start a timer to send alternating requestedPinValues to blink the LED
    if (response.Status == AppServiceResponseStatus.Success)
    {
        timer = ThreadPoolTimer.CreatePeriodicTimer(this.Tick, TimeSpan.FromMilliseconds(500));
    }
}
```

If everything connects without an error then the timer callback will toggle the value of the LED each time the timer event handler is called.

```csharp
if (requestedPinValue.Equals("High"))
{
    requestedPinValue = "Low";
}
else
{
    requestedPinValue = "High";
}
var message = new ValueSet();
message["requestedPinValue"] = requestedPinValue;
await connection.SendMessageAsync(message);
```

Remember that we connected the other end of the LED to the 3.3 Volts power supply, so we need to drive the pin to low to have current flow into the LED.
