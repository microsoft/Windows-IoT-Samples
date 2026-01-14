# NXP SPI Sample Project

This project is a C# application developed in Visual Studio, designed to validate the SPI bus on the NXP i.MX8M Plus board. It communicates with a BMP280 pressure and temperature sensor using the SPI protocol.

## Features

- **SPI Communication**: The application uses the SPI protocol for communication with the BMP280 pressure and temperature sensor.

- **Sensor Initialization**: The BMP280 sensor is initialized by powering on and enabling the pressure and temperature channels.

- **Sensor Data Reading**: The application reads data from the sensor every 500 milliseconds and updates the user interface with the new sensor values.

## Code Structure

The `MainPage` class is the core of the application. It includes methods for initializing the SPI device, initializing the sensor, reading data from the sensor, and updating the user interface with the sensor values.

## Usage

To run this project, ensure that Visual Studio is installed on your machine. Open the `.sln` file in Visual Studio, build the solution, and run the project. 

To run this application on an NXP device, ensure that the NXP device is on the same network as your host machine and that Visual Studio remote tools are installed. Build this application for ARM64, set the debug mode to 'remote debug', and set the target machine to the IP address of the NXP device.

Please note that you will need to connect the BMP280 sensor to a breadboard, and use male-to-female jumper cables to connect the corresponding ports on the sensor to the NXP board. The NXP Plus board's 40-pin I/O connector is labeled with the same names.

## Key Aspects of the Code

A crucial aspect of this code is the use of the friendly name 'SPI2' to access the SPI bus. This is specific to NXP - any other name would not work, as the SPI bus identifier is unique to the hardware and software configuration of the NXP device. The line `SpiDevice.GetDeviceSelector("SPI2")` in the `InitSPI` method is particularly important as it uses the 'SPI2' name to access the SPI bus on the NXP device.

## Note

This project is designed to work with a BMP280 pressure and temperature sensor. You will need to have this sensor connected to your machine in order to run the project. Always ensure to follow the correct setup and connection procedures to avoid any hardware issues.