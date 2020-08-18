# Interop Console App with Linux Edge Module

## Introduction
When running Linux container modules on Windows there are times when you may want bidirectional communication between an Linux module and a Windows process both running on a local Windows IoT device.  The communication methodology presented in this sample can be adapted to enable many difference scenarios, including:
1. A Windows application that provides a graphical user experience that is backed by business logic running in a custom Azure IoT Edge Linux module.
2. A Windows process implemented as a hardware proxy that sends data from specific Windows hardware into a custom Azure IoT Edge Linux module for analysis. 

## Prerequisites
To exercise this sample you will need the following
* An [Azure Subscription](https://azure.microsoft.com/free/) in which you have rights to deploy resources.  
* A device with Windows 10 version 1809 or later (build 17763 or later)¹
    > ¹ Windows 10 Editions Supported
    > * Windows 10 Professional
    > * Windows 10 Enterprise
    > * Windows 10 IoT Enterprise

* Setup of Azure IoT Edge for Linux on Windows using one of the two options below

    1.  Setup WSL2 with Ubuntu 18.04 and install Azure IoT Edge for Linux 
        * See [Linux modules with Azure IoT Edge on Windows 10 IoT Enterprise](https://aka.ms/winiot-low) blog

    1. Setup a standard Linux virtual machine and install Azure IoT Edge for Linux
        * See [How To Install Ubuntu 18.04 LTS On Windows 10 With Hyper-V](https://www.osstuff.com/how-to-install-ubuntu-18-04-lts-on-windows-10-with-hyper-v/) article on OSStuff.com.
        * See [Install the Azure IoT Edge runtime on Debian-based Linux system](https://docs.microsoft.com/azure/iot-edge/how-to-install-iot-edge-linux) documentation


## Instructions

[Step 1 - Setup Development Environment](./Documentation/Setup%20Development%20Environment.MD)   
[Step 2 - Setup Azure Resources](./Documentation/Setup%20Azure%20Resources.MD)  
[Step 3 - Develop and publish the IoT Edge Linux module](./Documentation/Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
[Step 4 - Develop the Windows C# Console Application](./Documentation/Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
[Step 5 - Create Certificates for Authentication](./Documentation/Create%20Certificates%20for%20Authentication.MD)  
[Step 6 - Configuring the IoT Edge Device](./Documentation/Configuring%20the%20IoT%20Edge%20Device.MD)  
[Step 7 - Deploy the Modules onto the IoT Edge Device](./Documentation/Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
[Step 8 - Run the Console Application](./Documentation/Run%20the%20Console%20Application.MD)  

## Feedback
If you have problems with this sample, please post an issue in this repository.
