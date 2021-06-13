
# Serial UART 

We'll create a simple app that allows communication between a desktop and an IoT device over a serial interface.

### Load the project in Visual Studio

Make a copy of the folder on your disk and open the project from Visual Studio 2019.

This app is a Universal Windows app and will run on both the PC and your IoT device.

### Wiring the serial connection 

You have two options for wiring up your board:

1. using the On-board UART controller
2. using a USB-to-TTL adapter cable such as [this one](http://www.adafruit.com/products/954).

#### <a name="UpBoard_UART"></a>On-board UART (UpBoard)

The UpBoard has an on-board UART. See the [UpBoard pin mapping page](../../Resources/PinMappingsUpBoard.png) for more details on the UpBoard GPIO pins. 

* UART uses GPIO pins 6, 8, 10  

Make the following connections:

* Insert the USB end of the USB-to-TTL cable into a USB port on the PC
* Connect the GND wire of the USB-to-TTL cable to Pin 6 (GND) on the Upboard
* Connect the RX wire (white) of the USB-to-TTL cable to Pin 8 (TX) on the  Upboard
* Connect the TX wire (green) of the USB-to-TTL cable to Pin 10 (RX) on the Upboard

*Note: Leave the power wire of the USB-to-TTL cable unconnected.*

[UART](../../Resources/SiLabs-UART.png)

### <a name="USB_TTL_Adapter"></a>Using USB-to-TTL Adapter

**Note: Only USB-to-TTL cables and modules with Silicon Labs chipsets are natively supported on UpBoard.**

You will need:

* 1 X USB-to-TTL module (This is what we will connect to our Upboard. We used [this Silicon Labs CP2102 based USB-to-TTL module](http://www.amazon.com/gp/product/B00LODGRV8))

* 1 X USB-to-TTL cable (This will connect to our PC. We used [this one](http://www.adafruit.com/products/954))

Make the following connections:

* Insert the USB end of the USB-to-TTL **cable** into a USB port on the PC

* Insert the USB end of the USB-to-TTL **module** into a USB port on the Upboard 

* Connect the GND pin of the USB-to-TTL **module** to the GND wire of the USB-to-TTL cable 

* Connect the RX pin of the USB-to-TTL **module** to the TX wire (green) of the USB-to-TTL cable

* Connect the TX pin of the USB-to-TTL **module** to the RX wire (white) of the USB-to-TTL cable

Leave the power pin of the USB-to-TTL cable unconnected. It is not needed.

Below is an image of our USB-to-TTL module connected to a USB port in our Upboard. The GND, TX, and RX pins of the module are connected to the GND, RX, TX wires of the USB-to-TTL cable that is connected to our PC.

[CP 2102 Connections](../../Resources/CP2102_Connections_500.png)

### Deploy and Launch the SerialSample App

Now that our UpBoard is connected, let's setup and deploy the app.

1. Navigate to the SerialSample source project. 

2. Make two separate copies of the app. We'll refer to them as the 'Device copy' and 'PC copy'.

3. Open two instances of Visual Studio 2019 on your PC. We'll refer to these as 'Instance A' and 'Instance B'.

4. Open the Device copy of the SerialSample app in VS Instance A.

5. Open the PC copy of the SerialSample app in VS Instance B.

6. In VS Instance A, configure the app for deployment to your UpBoard.
	
	*For UpBoard, set the target architecture to 'x64'

7. In VS Instance B, set the target architecture to 'x64'. This will be the instance of the sample we run on the PC.

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
 
  If you are using UPBOARD, you have to setup the BIOS UART configuration.

### BIOS Settings for UPBOARD

Steps to follow:
 
(1)	After power on the Upboard, Press Del or F7 to enter the BIOS setting.
 
(2)	Under the "Boot -> OS Image ID" Tab:
    Select "Windows 10 IoT Core".
 
(3)	Under the "Advance" Tab:
    Select "Hat Configuration" and make "LPSS HSUART #1 Support" as "Enabled".

If you need guidance click Link: [here](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

 Click the Start button to search for the app by name, and then launch it.

### Using the SerialSample App 

When the SerialSample app is launched on the PC, a window will open with the user interface similar to the screenshot shown below. When launched on the UpBoard, the SerialSample will display the user interface shown below on the entire screen.

[Serial Sample on PC](../../Resources/SerialSampleRunningPC.png)

#### Selecting a Serial Device

When the SerialSample app launches, it looks for all the serial devices that are connected to the device. The device ids of all the serial devices found connected to the device will be listed in the top ListBox of the SerialSample app window.

Select and connect to a serial device on the PC and UpBoard by doing the following:

1. Select the desired serial device by clicking on the device ID string in the top ListBox next to "Select Device:". 

	* On the PC, the device ID for the USB-to-TTL cable connected in this example begins with '\\?\USB#VID_067B'.
	
	* On the UpBoard, if using the USB-to-TTL adapter module, select the device ID that begins with **\\?\USB#**. For the USB-to-TTL module used in this example, the device ID should begin with '\\?\USB#VID_10C4'.

2. Click 'Connect'.	

The app will attempt to connect and configure the selected serial device. When the app has successfully connected to the attached serial device it will display the configuration of the serial device. By default, the app configures the serial device for 9600 Baud, eight data bits, no parity bits and one stop bit (no handshaking).

[Connect device](../../Resources/SerialSampleRunningPC_ConnectDevice.png)

#### Sending and Receiving Data

After connecting the desired serial device in the SerialSample apps running on both the PC and the UpBoard we can begin sending and receiving data over the serial connection between the two devices.

To send data from one device to the other connected device do the following:

1. Choose a device to transmit from. On the transmit device, type the message to be sent in the "Write Data" text box. For our example, we typed "Hello World!" in the "Write Data" text box of the SerialSample app running on our PC.

2. Click the 'Write' button.

The app on the transmitting device will display the sent message and "bytes written successfully!" in the status text box in the bottom of the app display.

[Send message](../../Resources/SendMessageB.png)

The device that is receiving the message will automatically display the text in the 'Read Data:' window.

**KNOWN ISSUES:**

* When connecting to the USB-to-TTL device for the first time from the IoT Device, you may see the error "Object was not instantiated" when you click on `Connect`. If you see this, un-plug the device, plug it back in and refresh the connection or redeploy the app.
* If you have more than one Silicon Labs USB-to-TTL devices connected to your IoT device, only the device that was first connected will be recognized. In order to run this sample, connect only one device
* When connecting USB-to-TTL device to UpBoard, use a powered USB hub or the bottom USB port


### Let's look at the code

The code for this sample uses the [Windows.Devices.SerialCommunication](https://msdn.microsoft.com/en-us/library/windows.devices.serialcommunication.aspx) namespace. 

The SerialDevice class will be used to enumerate, connect, read, and write to the serial devices connected to the device. 

**NOTE:** The SerialDevice class can be used only for supported USB-to-TTL devices (on PC and UpBoard).

For accessing the serial port, you must add the **DeviceCapability** to the **Package.appxmanifest** file in your project. 

You can add this by opening the **Package.appxmanifest** file in an XML editor (Right Click on the file -> Open with -> XML (Text) Editor) and adding the capabilities as shown below:

    Visual Studio 2019 has a known bug in the Manifest Designer (the visual editor for appxmanifest files) that affects the serialcommunication capability. If your appxmanifest adds the serialcommunication capability, modifying your appxmanifest with the designer will corrupt your appxmanifest (the Device xml child will be lost). You can workaround this problem by hand editing the appxmanifest by right-clicking your appxmanifest and selecting View Code from the context menu.

``` xml
  <Capabilities>
    <DeviceCapability Name="serialcommunication">
      <Device Id="any">
        <Function Type="name:serialPort" />
      </Device>
    </DeviceCapability>
  </Capabilities>
```

### Connect to selected serial device

This sample app enumerates all serial devices connected to the device and displays the list in the **ListBox** ConnectDevices. The following code connects and configure the selected device ID and creates a **SerialDevice** object. 

```csharp
private async void comPortInput_Click(object sender, RoutedEventArgs e)
{
    var selection = ConnectDevices.SelectedItems; // Get selected items from ListBox

    // ...

    DeviceInformation entry = (DeviceInformation)selection[0];         

    try
    {                
        serialPort = await SerialDevice.FromIdAsync(entry.Id);

        // ...

        // Configure serial settings
        serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
        serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);                
        serialPort.BaudRate = 9600;
        serialPort.Parity = SerialParity.None;
        serialPort.StopBits = SerialStopBitCount.One;
        serialPort.DataBits = 8;

        // ...
    }
    catch (Exception ex)
    {
        // ...
    }
}
```

### Perform a read on the serial port

Reading input from serial port is done by **Listen()** invoked right after initialization of the serial port. We do this in the sample code by creating an async read task using the **DataReader** object that waits on the **InputStream** of the **SerialDevice** object. 

Due to differences in handling concurrent tasks, the implementations of **Listen()** in C# and C++ differ:

* C# allows awaiting **ReadAsync()**. All we do is keep reading the serial port in an infinite loop interrupted only when an exception is thrown (triggered by the cancellation token).

```csharp

private async void Listen()
{
    try
    {
        if (serialPort != null)
        {
            dataReaderObject = new DataReader(serialPort.InputStream);

            // keep reading the serial input
            while (true)
            {
                await ReadAsync(ReadCancellationTokenSource.Token);
            }
        }
    }
    catch (Exception ex)
    {
        ...
    }
    finally
    {
        ...
    }
}

private async Task ReadAsync(CancellationToken cancellationToken)
{
    Task<UInt32> loadAsyncTask;

    uint ReadBufferLength = 1024;

    // If task cancellation was requested, comply
    cancellationToken.ThrowIfCancellationRequested();

    // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
    dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

    // Create a task object to wait for data on the serialPort.InputStream
    loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

    // Launch the task and wait
    UInt32 bytesRead = await loadAsyncTask;
    if (bytesRead > 0)
    {
        rcvdText.Text = dataReaderObject.ReadString(bytesRead);
        status.Text = "bytes read successfully!";
    }            
}
```

* C++ does not allow awaiting **ReadAsync()** in Windows Runtime STA (Single Threaded Apartment) threads due to blocking the UI. In order to chain continuation reads from the serial port, we dynamically generate repeating tasks via "recursive" task creation - "recursively" call **Listen()** at the end of the continuation chain. The "recursive" call is not a true recursion. It will not accumulate stack since every recursive is made in a new task.

``` c++

void MainPage::Listen()
{
    try
    {
        if (_serialPort != nullptr)
        {
            // calling task.wait() is not allowed in Windows Runtime STA (Single Threaded Apartment) threads due to blocking the UI.
            concurrency::create_task(ReadAsync(cancellationTokenSource->get_token()));
        }
    }
    catch (Platform::Exception ^ex)
    {
        ...
    }
}

Concurrency::task<void> MainPage::ReadAsync(Concurrency::cancellation_token cancellationToken)
{
    unsigned int _readBufferLength = 1024;
    
    return concurrency::create_task(_dataReaderObject->LoadAsync(_readBufferLength), cancellationToken).then([this](unsigned int bytesRead)
    {
        if (bytesRead > 0)
        {
            rcvdText->Text = _dataReaderObject->ReadString(bytesRead);
            status->Text = "bytes read successfully!";

            /*
            Dynamically generate repeating tasks via "recursive" task creation - "recursively" call Listen() at the end of the continuation chain.
            The "recursive" call is not true recursion. It will not accumulate stack since every recursive is made in a new task.
            */

            // start listening again after done with this chunk of incoming data
            Listen();
        }
    });
}
```

### Perform a write to the serial port

When the bytes are ready to be sent, we write asynchronously to the **OutputStream** of the **SerialDevice** object using the **DataWriter** object.

```csharp
private async void sendTextButton_Click(object sender, RoutedEventArgs e)
{	
    // ...
	
    // Create the DataWriter object and attach to OutputStream   
    dataWriteObject = new DataWriter(serialPort.OutputStream);

    //Launch the WriteAsync task to perform the write
    await WriteAsync();   
	
    // ..

    dataWriteObject.DetachStream();
    dataWriteObject = null;	
}

private async Task WriteAsync()
{
    Task<UInt32> storeAsyncTask;

    // ...
	
    // Load the text from the sendText input text box to the dataWriter object
    dataWriteObject.WriteString(sendText.Text);                

    // Launch an async task to complete the write operation
    storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

    // ...    
}
```

### Cancelling Read

You can cancel the read operation by using **CancellationToken** on the Task. Initialize the **CancellationToken** object and pass that as an argument to the read task.

```csharp

private async void comPortInput_Click(object sender, RoutedEventArgs e)
{
    // ...

    // Create cancellation token object to close I/O operations when closing the device
    ReadCancellationTokenSource = new CancellationTokenSource();
	
    // ...	
}

private async void rcvdText_TextChanged(object sender, TextChangedEventArgs e)
{
    // ...

    await ReadAsync(ReadCancellationTokenSource.Token);

    // ...	
}

private async Task ReadAsync(CancellationToken cancellationToken)
{
    Task<UInt32> loadAsyncTask;

    uint ReadBufferLength = 1024;

    cancellationToken.ThrowIfCancellationRequested();
    
    // ...
	
}
 
private void CancelReadTask()
{         
    if (ReadCancellationTokenSource != null)
    {
        if (!ReadCancellationTokenSource.IsCancellationRequested)
        {
            ReadCancellationTokenSource.Cancel();
        }
    }         
}
```

### Closing the device

When closing the connection with the device, we cancel all pending I/O operations and safely dispose of all the objects. 

In this sample, we proceed to also refresh the list of devices connected.

```csharp
private void closeDevice_Click(object sender, RoutedEventArgs e)
{
    try
    {
        CancelReadTask();
        CloseDevice();
        ListAvailablePorts(); //Refresh the list of available devices
    }
    catch (Exception ex)
    {
       // ...
    }          
}    

private void CloseDevice()
{            
    if (serialPort != null)
    {
        serialPort.Dispose();
    }    

    // ...
}    
```


To summarize:

* First, we enumerate all the serial devices connected and allow the user to connect to the desired one using device ID

* We create an asynchronous task for reading the **InputStream** of the **SerialDevice** object.

* When the user provides input, we write the bytes to the **OutputStream** of the **SerialDevice** object.

* We add the ability to cancel the read task using the **CancellationToken**.

* Finally, we close the device connection and clean up when done.
