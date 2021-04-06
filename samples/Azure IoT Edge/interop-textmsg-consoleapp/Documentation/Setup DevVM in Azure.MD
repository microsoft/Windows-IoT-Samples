# Interop Console App with Linux Edge Module
## Progress

- [x] [Introduction](../README.md)  
- [ ] **Step 1 - Setup Development Environment**
- [ ] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [ ] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [ ] [Step 4 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [ ] [Step 5 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
Over the course of walk-through for this sample we will be performing various developer tasks including coding, compiling, configuring, and deploying IoT Edge module and IoT devices. To provide a common basis for the performing these tasks, we recommend the use of the [DevVM scripts](https://github.com/Azure-Samples/IoTEdgeAndMlSample/blob/master/DevVM) in the [IoTEdgeandMLSample](https://github.com/Azure-Samples/IoTEdgeAndMlSample) to create an configure an Azure Virtual Machine (VM) specifically for this walk-through.

Where would you like to create your developer environment?  

- [x] Virtual Machine in Azure 
- [ ] [Virtual Maching on Local PC](./Setup%20DevVM%20on%20Local%20PC.MD)
    
---
# Step 1: Setup DevVM in Azure   

1. Download Scripts
Download the DevVM scripts from the following project to your local machine:   
https://github.com/Azure-Samples/IoTEdgeAndMlSample/blob/master/DevVM  

    > **Note:** To run these scripts you will need an [Azure Subscription](https://azure.microsoft.com/en-us/free/) in which you have rights to deploy resources.

1.  Create an Azure VM for Dev Environment

    Open a **Powershell** and run **Create-AzureDevVm.ps1**
    ```powershell
    .\Create-AzureDevVm.ps1
    ```
     When prompted provide the following:
    - **Azure Subscription ID:** found in the Azure Portal 
    - **Resource Group Name:** memorable name for grouping the resources for your walk-through
    - **Location:** Azure location where the virtual machine will be created (e.g. US West 2, North Europe see full list) 
    - **AdminUsername:** the username with which you will log into the virtual machine
    - **AdminPassword:** the password to set for the AdminUsername on the VM
       
    > Note:  
    Save these pieces of data as you will need to supply them multiple times throughout this walk-through.

    The script confirms the information for the creation of your VM press <kbd>y</kbd> or <kbd>Enter</kbd> to continue

    The script will run for several minutes as it executes the steps:
    - Create the Resource Group if it does not exist
    - Deploy the virtual machine
    - Enable Hyper-V on the VM
    - Install software need for development and clone the sample repository
    - Restart the VM
    - Create an RDP file on your desktop for connecting to the VM 

    > Note:  
    The VM is created with a default shutdown schedule set for 1900 PST. Navigate to the VM in the Azure Portal and choose Auto-shutdown from the side navigator to update the timing.

1. **Install Visual Studio Code extensions**  
    * Log in to the VM using the RDP file created above
    * In a **PowerShell** window navigate to `C:\source\IoTEdgeAndMlSample\ConfigureVM`
    * Run the [Set-ExecutionPolicy](https://docs.microsoft.com/powershell/module/microsoft.powershell.security/set-executionpolicy?view=powershell-7) cmdlet to allow script execution with the following command:

        ```powershell
        Set-ExecutionPolicy Bypass -Scope CurrentUser -Force`
        ```

    * Run the script **Enable-CodeExtensions.ps1**
        ```powershell
        .\Enable-CodeExtensions.ps1
        ```

        The script will install the following extensions:
        - Azure IoT Tools
        - Python
        - Azure
        - C#
        - Docker
        - PowerShell

Go to [Next Step](./Setup%20Azure%20Resources.MD)  
