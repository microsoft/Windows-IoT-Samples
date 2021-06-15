---
page_type: sample
urlFragment: temp-force-sensor
languages:
  - csharp
products:
  - Windows 10
  - Windows IoT 
  - Windows 10 IoT Enterprise
description: Control a basic temperature and force sensor for Windows 10 IoT Enterprise.
---

# Temperature and force sensor
This sample uses SPI communication. 

A temperature/force sensor is connected to an ADC, the ADC is connected to the UP Board through SPI Pins. 

The ADC converts the analog sensor output to a digital value that is then read by the UP Board using SPI. 

The value read from the ADC is displayed on the screen attached to the UP Board.

This is basically a simplified version of Potentiometer sensor sample, which has an LED light as an extra output.

You can also use a Force sensor in this sample. Try to press the Force sensor gentle or hard to see the data output difference.

This sample only has C# version.

## Read before start
This sample assumes that UP Board has already been set up as below:

- UP Board has been connected to HDMI monitor
- An Ethernet cable has been plugged to UP Board
- UP Board has been powered on

## Parts needed

- [1 MCP3002 10-bit ADC](http://www.digikey.com/product-detail/en/MCP3002-I%2FP/MCP3002-I%2FP-ND/319412) or [1 MCP3208 12-bit ADC](http://www.digikey.com/product-search/en?KeyWords=mcp3208%20ci%2Fp&WT.z_header=search_go)
- [1 TMP36 Temperature sensor](http://www.digikey.com/product-detail/en/TMP36GT9Z/TMP36GT9Z-ND/820404) or [1 FSR 402 Force sensor](http://www.digikey.com/product-detail/en/30-81794/1027-1001-ND/2476468)
- UP Board
- 1 breadboard and a couple of wires
- HDMI Monitor

## Parts Review

* MCP3002 or MCP3208

Below are the pinouts of the MCP3002 and MCP3208 analog-to-digital converters (ADC) used in this sample.

![Electrical Components](../Resources/MCP3002.png)
![Electrical Components](../Resources/MCP3208.png)

* UP Board

 ![UP Board Pinout](../Resources/Upboard_Pinout.png)

## Parts Connection

1. Connect the TMP36 temperature Sensor to the MCP3002; `Sensor output pin` (the middle pin) should be connected to `CH0` on the MCP3002;

If you are using a [Force sensor](http://www.digikey.com/product-detail/en/30-81794/1027-1001-ND/2476468) which only has two legs, set the left leg to 5V,
and connect the other Leg to `CH0` on MCP3002

Detailed connection:

![Overall Schematics](../Resources/temp_mcp3002.png);
![Overall Schematics](../Resources/force_mcp3002.png);

With each model of UP Board, the pin layout might be a little different. But the pin connection with MCP3002 should be as below:

- MCP3002: VDD/VREF - 5V on UP Board
- MCP3002: CLK - "SPI2 SCLK" on UP Board
- MCP3002: Dout - "SPI2 MISO" on UP Board
- MCP3002: Din - "SPI2 MOSI" on UP Board
- MCP3002: CS/SHDN - "SPI2 CS0" on UP Board
- MCP3002: DGND - GND on UP Board
- MCP3002: CH0- Sensor Output Pin

2. **Alternative: If you are using MCP3208** Connect the temperature Sensor to MCP3208; `Sensor output pin` (the mid pin) should be connected to `CH0` on MCP3208.

Detailed connection:

![Overall Schematics](../Resources/OverallCon_mcp3208.png )

With each model of UP Board, the pin layout might be a little different.
But the pin connection with MCP3208 should be as below:

- MCP3208: VDD - 5V on UP Board
- MCP3208: VREF - 5V on UP Board
- MCP3208: CLK - "SPI2 SCLK" on UP Board
- MCP3208: Dout - "SPI2 MISO" on UP Board
- MCP3208: Din - "SPI2 MOSI" on UP Board
- MCP3208: CS/SHDN - "SPI2 CS0 on UP Board
- MCP3208: DGND - GND on UP Board

## Look at the code

Let's go through the code. We use a timer in the sample, and each time the 'Tick' event is called,
we read the sensor data through ADC, and the value will be displayed on the screen.

* Timer Code
Setup timer in C#:
```csharp
public MainPage()
{
	// ...

	this.timer = new DispatcherTimer();
	this.timer.Interval = TimeSpan.FromMilliseconds(500);
	this.timer.Tick += Timer_Tick;
	this.timer.Start();

	// ...
}
private void Timer_Tick(object sender, object e)
{
	DisplayTextBoxContents();
}
```

* Initialize SPI pin
```csharp
private async void InitSPI()
{
    try
    {
        var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
        settings.ClockFrequency = 1000000;// For UP Board use 1MHz and For Rpi use 500KHz
        settings.Mode = SpiMode.Mode0; //Mode3;

        string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
        var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
        SpiDisplay = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
    }

    /* If initialization fails, display the exception and stop running */
    catch (Exception ex)
    {
        throw new Exception("SPI Initialization Failed", ex);
    }
}
```

* Read the sensor data through SPI communication

```csharp

/*UP Board  Parameters*/
private const string SPI_CONTROLLER_NAME = "SPI2";  /*  For UP Board, use SPI2. and For Raspberry Pi 2, use SPI0     */
private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the UP Board        */

/*Channel configuration for MCP3208, Uncomment this if you are using MCP3208*/

// byte[] readBuffer = new byte[3]; /*this is defined to hold the output data*/
// byte[] writeBuffer = new byte[3] { 0x06, 0x00, 0x00 };//00000110 00; /* It is SPI port serial input pin, and is used to load channel configuration data into the device*/

/*Channel configuration for MCP3002, Uncomment this if you are using MCP3002*/
byte[] readBuffer = new byte[3]; /*this is defined to hold the output data*/
byte[] writeBuffer = new byte[3] { 0x68, 0x00, 0x00 };//00001101 00; /* It is SPI port serial input pin, and is used to load channel configuration data into the device*/

private SpiDevice SpiDisplay;

// create a timer
private DispatcherTimer timer;
int res;

public void DisplayTextBoxContents()
{
    SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
    res = convertToInt(readBuffer);
    textPlaceHolder.Text = res.ToString();

}
```

* Convert sensor bit data to a number

```csharp
/* This is the conversion for MCP3208 which is a 12 bits output; Uncomment this if you are using MCP3208 */
// public int convertToInt(byte[] data)
// {
//    int result = data[1] & 0x0F;
//    result <<= 8;
//    result += data[2];
//    return result;
// }
/* */

/* This is the conversion for MCP3002 which is a 10 bits output; Uncomment this if you are using MCP3002 */
public int convertToInt(byte[] data)
{
    int result = data[0] & 0x03;
    result <<= 8;
    result += data[1];
    return result;
}
```

## Deploy the sample

* Choose `Release` and `x64` configuration.
* Compile the Solution file

### [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)

### [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)
 
### BIOS Settings for UP Board

If you are using UP Board, you have to setup the BIOS SPI configuration.

1. Once you power on the UP board, select the **Del** or **F7** key on your keyboard to enter the BIOS setting.

1. Navigate to **Boot** > **OS Image ID** tab, and select **Windows 10 IoT Core**.
	
1. Navigate to the **Advance** tab and select the **Hat Configuration** and select **LPSS SPISupport** as **Enabled**.

1. For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

## Run the App

Click the Start button to search for the app by name, and then launch it.

If you are using Temp sensor, you can try to hold the sensor or apply some heat on it to see how the output change. If you are using Force sensor, you can hold it hard or gentle to see
how the output change on the screen. You can also switch the sensor to a light sensor to play around with it.

[Deploy temperature sensor](../Resources/Deploy.png)

## Additional Notes

Make sure that LowLevel Capabilities in set in PackageAppManifest.
* To do that go to Package.appxmanifesto and view the code
* Under Capabilities if you can find "DeviceCapability Name="lowLevel"/" then your lowLevel Capabilities is enabled.
* If this line "DeviceCapability Name="lowLevel"/" is not present then add it to enable the LowLevel mode and save the PackageAppManifest.
