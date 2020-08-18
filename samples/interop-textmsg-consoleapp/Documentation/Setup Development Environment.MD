# Interop Console App with Linux Edge Module
## Progress

- [ ] [**Step 1 - Setup Development Environment**](./Setup%20Development%20Environment.MD)   
- [ ] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [ ] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [ ] [Step 4 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] [Step 5 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
# Setup Development Environment   

## Introduction

Over the course of walk-through for this sample we will be performing various developer tasks including coding, compiling, configuring, and deploying IoT Edge module and IoT devices. To provide a common basis for the performing these tasks, we recommend the use of the scripts in [this folder](https://github.com/Azure-Samples/IoTEdgeAndMlSample/blob/master/DevVM) to create an configure an Azure Virtual Machine (VM) specifically for this walk-through.

## Prerequisites

To run these scripts you will need an [Azure Subscription](https://azure.microsoft.com/en-us/free/) in which you have rights to deploy resources.

## Running the scripts

### Create the VM

1. Download the scripts to a local machine from the following project:   
https://github.com/Azure-Samples/IoTEdgeAndMlSample/blob/master/DevVM

1. Open a Powershell window and run `.\Create-AzureDevVm.ps1`

1. When prompted provide:
    - **Azure Subscription ID:** found in the Azure Portal 
    - **Resource Group Name:** memorable name for grouping the resources for your walk-through
    - **Location:** Azure location where the virtual machine will be created (e.g. US West 2, North Europe see full list) 
    - **AdminUsername:** the username with which you will log into the virtual machine
    - **AdminPassword:** the password to set for the AdminUsername on the VM
      
    > Note  
    > Make a note of these pieces of data as you will need to supply them multiple times throughout this walk-through.

1. Login to your Azure account as needed

1. The script confirms the information for the creation of your VM press ‘y’ or ‘Enter’ to continue

1. The script will run for several minutes as it executes the steps:
    - Create the Resource Group if it does not exist
    - Deploy the virtual machine
    - Enable Hyper-V on the VM
    - Install software need for development and clone the sample repository
    - Restart the VM
    - Create an RDP file on your desktop for connecting to the VM 

    > Note: the VM is created with a default shutdown schedule set for 1900 PST. Navigate to the VM in the Azure Portal and choose Auto-shutdown from the side navigator to update the timing.

### Install Visual Studio Code extensions

1. Log in to the VM using the RDP file created above

1. In a PowerShell window navigate to **C:\source\IoTEdgeAndMlSample\ConfigureVM**

1. Execute the Set-ExecutionPolicy cmdlet to allow script execution with the command `Set-ExecutionPolicy Bypass -Scope CurrentUser -Force`

1. Run the script `.\Enable-CodeExtensions.ps1`

1. The script install these extensions:
    - Azure IoT Tools
    - Python
    - Azure
    - C#
    - Docker
    - PowerShell

## Alternate setup using local machine

The scripts can be run on a machine outside of Azure.  This requires a machine or virtual machine that is capable of [nested virtualization](https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/user-guide/nested-virtualization)

1. Log into the machine you wish to configure

1. Open PowerShell as an administrator and navigate to directory containing the scripts

1. Execute the Set-ExecutionPolicy cmdlet to allow script execution with the command `Set-ExecutionPolicy Bypass -Scope CurrentUser -Force`

1. In PowerShell run the scripts in the following order:

     ```powershell
    Enable-HyperV.ps1
    Install-Chocolatey.ps1
    Install-DevMachineSoftware.ps1
    ```

1. Restart the machine with the command `Restart-Computer`

1. Enable Visual Studio Code extensions with the command `.\Enable-CodeExtensions.ps1`

Go to [Next Step](./Setup%20Azure%20Resources.MD)  