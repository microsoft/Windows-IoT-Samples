// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace NXPGPIOSample
{
    public sealed partial class MainPage : Page
    {
        private int[] LED_PINS = new int[] {147, 146, 141, 140, 139, 138, 137, 136, 87, 86, 85, 84, 83 };
        private GpioPin[] pins;
        private GpioPinValue[] pinValues;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            InitGPIO();
            if (pins != null)
            {
                timer.Start();
            }
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pins = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pins = new GpioPin[LED_PINS.Length];
            pinValues = new GpioPinValue[LED_PINS.Length];
            for (int i = 0; i < LED_PINS.Length; i++)
            {
                pins[i] = gpio.OpenPin(LED_PINS[i]);
                pinValues[i] = GpioPinValue.High;
                pins[i].Write(pinValues[i]);
                pins[i].SetDriveMode(GpioPinDriveMode.Output);
            }

            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private void Timer_Tick(object sender, object e)
        {
            for (int i = 0; i < LED_PINS.Length; i++)
            {
                if (pinValues[i] == GpioPinValue.High)
                {
                    pinValues[i] = GpioPinValue.Low;
                    pins[i].Write(pinValues[i]);
                    LED.Fill = redBrush;
                }
                else
                {
                    pinValues[i] = GpioPinValue.High;
                    pins[i].Write(pinValues[i]);
                    LED.Fill = grayBrush;
                }
            }
        }
    }
}
