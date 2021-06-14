---
page_type: sample
urlFragment: appservice-blinky
languages:
  - csharp
  - cpp
products:
  - Windows 10
  - Windows IoT 
  - Windows 10 IoT Enterprise
description: Create a simple Blinky app service and connect a LED to your Windows IoT Enterprise device.
---

# Using an app service to blink an LED
We’ll create a simple Blinky [app service](https://docs.microsoft.com/en-us/windows/uwp/launch-resume/how-to-create-and-consume-an-app-service) and connect a LED to your Windows IoT Enterprise device (Up-Board). We'll also create a simple app service client that blinks the LED. Be aware that the GPIO APIs are only available on the board, so this sample cannot run on your desktop.

## Set up your hardware
___
The hardware setup for this sample is the same as the Blinky sample.
Note that the app will not run successfully if it cannot find any available GPIO ports.

## Load the projects in Visual Studio
___

* You can find the source code for this sample by downloading a zip of all of our samples in Windows 10 IoT Enterprise folder and navigating to the `samples-develop\AppServiceBlinky`. 
* Make a copy of the folder on your disk and open the projects from Visual Studio.  
* BlinkyService.sln implements the app service and must be started first.  
* BlinkyClient.sln implements the app service client.
* Verify that the value of connection.PackageFamilyName in StartupTask.cs and MainPage.xaml.cs matches the value output in the output window by BlinkyService.
* If it does not have any value output, you will get the value by marking the breakpoint in debug mode for #StartupTask.cs at System.Diagnostics.Debug.WriteLine("Service closed. Status=" + args.Status.ToString()).

## Deploy your app

* * *

With the application open in Visual Studio 2019, set the architecture in the toolbar dropdown. If you’re building for UP Board, select `x64`.

### [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)

### [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)

### BIOS Settings for UP Board
If you are using an UP Board, you have to setup the BIOS GPIO configuration.

1. Once you power on the UP board, select the **Del** or **F7** key on your keyboard to enter the BIOS setting.

1. Navigate to **Boot** > **OS Image ID** tab, and select **Windows 10 IoT Core**.

1. Navigate to the **Advance** tab and select the **Hat Configuration** and select **GPIO Configuration in Pin Order**.

1. Configure the Pins you are using in the sample as **INPUT** or **OUTPUT**.

1. For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).
  
1. Click the Start button to search for the app by name, and then launch it.

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

## Additional Notes

Make sure that LowLevel Capabilities in set in PackageAppManifest.
* To do that go to Package.appxmanifesto and view the code
* Under Capabilities if you can find "DeviceCapability Name="lowLevel"/" then your lowLevel Capabilities is enabled.
* If this line "DeviceCapability Name="lowLevel"/" is not present then add it to enable the LowLevel mode and save the PackageAppManifest.
