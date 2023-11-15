using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NXPI2CSample
{
    public sealed partial class MainPage : Page
    {
        private const string I2C_CONTROLLER_NAME = "I2C3"; 
        private const int TCS34725_ADDRESS = 0x29;

        private I2cDevice tcs34725;
        private DispatcherTimer timer;
        private const byte TCS34725_ENABLE = 0x00;
        private const byte TCS34725_ENABLE_PON = 0x01; // Power on
        private const byte TCS34725_ENABLE_AEN = 0x02; // RGBC Enable

        // Change these to control the gain and integration time of the sensor, and 
        // uncomment the lines in 'InitialiseSensor' to set the values to the sensor

        //private const byte TCS34725_ATIME = 0x01; // RGBC time
        //private const byte TCS34725_INTEGRATIONTIME_700MS = 0x00; // Max count: 65535
        //private const byte TCS34725_CONTROL = 0x0F; // Gain control register
        //private const byte TCS34725_GAIN_60X = 0x03; // 60x gain


        public MainPage()
        {
            this.InitializeComponent();
            InitI2C();

            // Create a timer to update the sensor values every 500 milliseconds
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void InitI2C()
        {
            var settings = new I2cConnectionSettings(TCS34725_ADDRESS);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);
            var dis = await DeviceInformation.FindAllAsync(aqs);

            if (dis.Count == 0)
            {
                Debug.WriteLine("No I2C devices found.");
                return;
            }

            foreach (var device in dis)
            {
                Debug.WriteLine($"Device ID: {device.Id}");
                Debug.WriteLine($"Device Name: {device.Name}");
                Debug.WriteLine($"Device Kind: {device.Kind}");
                Debug.WriteLine($"Device Properties: {device.Properties}");
            }

            tcs34725 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

            if (tcs34725 == null)
            {
                Debug.WriteLine("Failed to connect to TCS34725 sensor.");
                return;
            }

            Debug.WriteLine("Connected to TCS34725 sensor.");

            // Initialize the sensor
            InitialiseSensor();
        }

        private void Timer_Tick(object sender, object e)
        {
            // Read data from the sensor
            byte[] data = new byte[8];
            tcs34725.WriteRead(new byte[] { 0x94 }, data);

            ushort clear = (ushort)(data[1] << 8 | data[0]);
            ushort red = (ushort)(data[3] << 8 | data[2]);
            ushort green = (ushort)(data[5] << 8 | data[4]);
            ushort blue = (ushort)(data[7] << 8 | data[6]);

            // Update the UI with the new sensor values
            ClearTextBlock.Text = $"Clear: {clear}";
            RedTextBlock.Text = $"Red: {red}";
            GreenTextBlock.Text = $"Green: {green}";
            BlueTextBlock.Text = $"Blue: {blue}";
        }

        private void write8(byte addr, byte cmd)
        {
            tcs34725.Write(new byte[] { addr, cmd });
        }

        private void InitialiseSensor()
        {
            // Turn on
            write8(TCS34725_ENABLE, TCS34725_ENABLE_PON);
            Task.Delay(3).Wait();
            write8(TCS34725_ENABLE, TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);

            // Integration Time
           // write8(TCS34725_ATIME, TCS34725_INTEGRATIONTIME_700MS);

            // Gain
           // write8(TCS34725_CONTROL, TCS34725_GAIN_60X);

            write8(0x80, 0x03);
        }
    }
}
