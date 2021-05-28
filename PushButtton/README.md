---
page_type: sample
urlFragment: push-button
languages:
  - csharp
products:
  - windows
  - windows-iot
  - windows-10-iot-Enterprise
description: Make an LED blink with the push of a button with Windows 10 IoT Enterprise.
---

# Push button

In this sample, we connect a push button to your Upboard and use it to control an LED. We use GPIO interrupts to detect when the button is pressed and toggle the LED in response.

![Push Button Image](../../PushButtonSample.png)

Also, be aware that the GPIO APIs are only available on Windows IoT Enterprise, so this sample cannot run on your desktop.

### Components

You will need the following components :

* [EG1311-ND Tactile Button](http://www.digikey.com/product-detail/en/320.02E11.08BLK/EG1311-ND/101397)

* [Red LED](http://www.digikey.com/product-detail/en/C5SMF-RJS-CT0W0BB1/C5SMF-RJS-CT0W0BB1-ND/2341832)

* [330 &#x2126; resistor](http://www.digikey.com/product-detail/en/CFR-25JB-52-330R/330QBK-ND/1636)

* Breadboard and several male-to-male for the Upboard.

### Connect the circuit to your device

Let's start by wiring up the components on a breadboard. Visit the corresponding **Upboard** sections below depending on your device.

#### Upboard

| Breadboard Diagram                                                                        | Schematic                                                                          |
| ----------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| ![Breadboard connections](../../Resources/Upboard_PushButton_bb.png)      | ![Circuit Schematic](../../Resources/images/Upboard_PushButton_schem.png) |


##### Connecting the LED

* Connect the cathode (the shorter leg) of the LED to Pin 31 (GPIO 11) of the Upboard

* Connect the anode (the longer leg) of the LED to one lead of the 330 &#x2126; resistor

* Connect the other end of the 330 &#x2126; resistor to Pin 1 (3.3V) on Upboard

##### Connecting the Push Button

* Connect one pin of the push button to Pin 29 (GPIO 10) of the Upboard

* Connect the other pin of the push button to one lead of the 330 &#x2126; resistor

* Connect the other end of the 330 &#x2126; resistor to ground

Here is the pinout of the Upboard

![Upboard pinout](../../Upboard_Pinout.png)


```csharp
private const int LED_PIN = 31;
private const int BUTTON_PIN = 29;
```

### Building and running the sample

1. Download a zip of all of our samples.
1. Open `samples-develop\PushButton\CS\PushButton.csproj` in Visual Studio.
1. If you have **Upboard**, Select `Release x64` for the target architecture.
1. Go to `Build -> Build Solution`.

### Generate an app package

Steps to follow :

 In Solution Explorer, open the solution for your application project.
 Right-click the project and choose Publish->Create App Packages (before Visual Studio 2019 version 16.3, the Publish menu is named Store).
 Select Sideloading in the first page of the wizard and then click Next.
 On the Select signing method page, select whether to skip packaging signing or select a certificate for signing. You can select a certificate from your local certificate store, select a certificate file, or create a new certificate. For an MSIX package to be installed on an end user's machine, it must be signed with a cert that is trusted on the machine.
 Complete the Select and configure packages page as described in the Create your app package upload file using Visual Studio section.

 If you need guidance click Link: [here](https://docs.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#generate-an-app-package).  
  
### Install your app package using an install script

Steps to follow :
 Open the *_Test folder.
 Right-click on the Add-AppDevPackage.ps1 file. Choose Run with PowerShell and follow the prompts.
 When the app package has been installed, the PowerShell window displays this message: Your app was successfully installed.

 If you need guidance click Link: [here](https://docs.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script).  
  
 Click the Start button to search for the app by name, and then launch it.

 If you are using UPBOARD, you have to setup the BIOS GPIO configuration.

### BIOS Settings for UPBOARD

Steps to follow:
 
(1)	After power on the Upboard, Press Del or F7 to enter the BIOS setting.
 
(2)	Under the "Boot -> OS Image ID" Tab:
    Select "Windows 10 IoT Core".
 
(3)	Under the "Advance" Tab:
    Select "Hat Configuration", then Click on "GPIO Configuration in Pin Order".

(4) Configure the Pins you are using in the sample as "INPUT" or "OUTPUT".

    In this sample make PIN 31 as "OUTPUT" and PIN 29 as "INPUT".

### Let's look at the code

First, we open the GpioPin resources we'll be using. The button is connected to
GPIO10 in active LOW configuration, meaning the signal will be HIGH when the
button is not pressed and the signal will go LOW when the button is pressed.
We'll be using the LED, connected to GPIO11, which is connected in
active LOW configuration, meaning driving the pin HIGH will turn off the LED
and driving the pin LOW will turn on the LED.

```csharp
buttonPin = gpio.OpenPin(BUTTON_PIN);
ledPin = gpio.OpenPin(LED_PIN);
```

We initialize the LED in the OFF state by first latching a HIGH value onto the
pin. When we change the drive mode to Output, it will immediately drive the
latched output value onto the pin. The latched output value is undefined when
we initially open a pin, so we should always set the pin to a known state
before changing it to an output. Remember that we connected the other end 
of the LED to 3.3V, so we need to drive the pin to low to have current flow into the LED.

```csharp
// Initialize LED to the OFF state by first writing a HIGH value
// We write HIGH because the LED is wired in a active LOW configuration
ledPin.Write(GpioPinValue.High); 
ledPin.SetDriveMode(GpioPinDriveMode.Output);
```

Next, we set up the button pin. For the Upboard, we take advantage of the fact that it has 
built-in pull up resistors that we can activate. We use the built-in pull up resistor so that we don't need to supply a resistor externally. 

```csharp
// Check if input pull-up resistors are supported
if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
	buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
else
	buttonPin.SetDriveMode(GpioPinDriveMode.Input);
```

Next we connect the GPIO interrupt listener. This is an event that will get
called each time the pin changes state. We also set the DebounceTimeout
property to 50ms to filter out spurious events caused by electrical noise.
Buttons are mechanical devices and can make and break contact many times on a
single button press. We don't want to be overwhelmed with events so we filter
these out.

```csharp
// Set a debounce timeout to filter out switch bounce noise from a button press
buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

// Register for the ValueChanged event so our buttonPin_ValueChanged 
// function is called when the button is pressed
buttonPin.ValueChanged += buttonPin_ValueChanged;
```

In the button interrupt handler, we look at the edge of the GPIO signal to
determine whether the button was pressed or released. If the button was
pressed, we flip the state of the LED.

```csharp
private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
{
	// toggle the state of the LED every time the button is pressed
	if (e.Edge == GpioPinEdge.FallingEdge)
	{
		ledPinValue = (ledPinValue == GpioPinValue.Low) ?
			GpioPinValue.High : GpioPinValue.Low;
		ledPin.Write(ledPinValue);
	}
```

We also want to update the user interface with the current state of the
pin, so we invoke an update operation on the UI thread. Capturing the result
of an async method in a local variable is necessary to suppress a compiler
warning when we don't want to wait for an asynchronous operation to complete.

```csharp
// need to invoke UI updates on the UI thread because this event
// handler gets invoked on a separate thread.
var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
	if (e.Edge == GpioPinEdge.FallingEdge)
	{
		ledEllipse.Fill = (ledPinValue == GpioPinValue.Low) ? 
			redBrush : grayBrush;
		GpioStatus.Text = "Button Pressed";
	}
	else
	{
		GpioStatus.Text = "Button Released";
	}
});
```

That's it! Each time you press the button, you should see the LED change
state.
