using System;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;

namespace SPITemperatureAndSensor
{
    public sealed partial class MainPage : Page
    {
        private SpiDevice SpiDevice;
        private BMP280 BMP280Sensor;

        public MainPage()
        {
            this.InitializeComponent();
            InitSPI();
        }

        private async void InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(0); // Use chip select line 0
                settings.ClockFrequency = 500000; // 500KHz clock rate
                settings.Mode = SpiMode.Mode0; // SPI mode 0

                string spiQuery = SpiDevice.GetDeviceSelector("SPI2"); // Find the selector string for the SPI bus controller
                var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery); // Find the SPI bus controller device with our selector string
                SpiDevice = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings); // Create an SpiDevice with our bus controller and SPI settings

                BMP280Sensor = new BMP280(SpiDevice);

                ReadSensor();
            }
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private async void ReadSensor()
        {
            while (true)
            {
                double temperature = BMP280Sensor.ReadTemperature();
                //double pressure = BMP280Sensor.ReadPressure();

                TemperatureTextBlock.Text = $"Temperature: {temperature} °C";
                //PressureTextBlock.Text = $"Pressure: {pressure} hPa";

                await Task.Delay(1000); // Wait for one second
            }
        }
    }

    public class BMP280
    {
        private SpiDevice SpiDevice;

        public BMP280(SpiDevice spiDevice)
        {
            this.SpiDevice = spiDevice;
        }

        public double ReadTemperature()
        {
            byte[] readBuffer = new byte[3]; // Buffer to hold read data
            byte[] writeBuffer = new byte[] { 0xFA }; // Command to read temperature

            SpiDevice.Write(writeBuffer); // Send the command
            SpiDevice.Read(readBuffer); // Read the response

            // Convert the data to temperature in degrees Celsius
            int rawTemp = readBuffer[1] << 8 | readBuffer[2];
            double temp = rawTemp / 16.0;

            return temp;
        }

        public double ReadPressure()
        {
            byte[] readBuffer = new byte[3]; // Buffer to hold read data
            byte[] writeBuffer = new byte[] { 0xF7 }; // Command to read pressure

            SpiDevice.Write(writeBuffer); // Send the command
            SpiDevice.Read(readBuffer); // Read the response

            // Convert the data to pressure in hPa
            int rawPressure = readBuffer[1] << 8 | readBuffer[2];
            double pressure = rawPressure / 16.0;

            return pressure;
        }
    }

}
