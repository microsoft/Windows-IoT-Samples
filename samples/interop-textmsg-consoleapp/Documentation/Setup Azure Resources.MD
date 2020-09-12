# Interop Console App with Linux Edge Module
## Progress

- [x] [Introduction](../README.md)  
- [x] [Step 1 - Setup Development Environment](./Setup%20DevVM.MD)   
- [ ] **Step 2 - Setup Azure Resources**  
- [ ] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [ ] [Step 4 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [ ] [Step 5 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
# Step 2: Set up Azure resources  
This part describes how to set up the required Azure entities with the [Azure Cloud Shell](https://docs.microsoft.com/azure/cloud-shell/overview). Alternatively you can use [Azure PowerShell](https://docs.microsoft.com/powershell/azure/) module. This should be executed on any Windows host other than the (emulated) target device. We propose using the VM that serves as development environment. 

1. [Determine Azure subscription Id](#GetSubscriptionId)  
1. [Set the subscription Id context](#SetAzContext)  
1. [Create an IoT Edge device](#CreateIoTEdgeDevice)  
1. [Create an IoT Downstream Device](#CreateDownstreamDevice)  
1. [Get Parent device Connection String](#GetParentConnectionString)  
1. [Get Downstream device Connection String](#GetDownstreamConnectionString)  
1. [Setup Container Registry](#ContainerRegistry)  

----
   <a name="GetSubscriptionId"></a>  
1. **Determine Azure subscription Id**  
   
   Use [Get-AzSubscription](https://docs.microsoft.com/powershell/module/az.accounts/get-azsubscription?view=azps-4.5.0) to list the subscriptions associated with your account then copy the value in the **Id** column associated with the Name of your subscription
   ```powershell
   Get-AzSubscription
   ```

    <a name="SetAzContext"></a>
1. **Set the subscription Id context**  
   
   Use the [Set-AzContext](https://docs.microsoft.com/powershell/module/az.accounts/set-azcontext) command to set the subscription ID context.  
   > Replace **Id** with the Id copied from above.
   ```powershell
   Set-AzContext -SubscriptionId "Id"
   ```

   <a name="CreateIoTEdgeDevice"></a>
1. **Create an IoT Edge Device**  
   Use the [Add-AzIoTHubDevice](https://docs.microsoft.com/powershell/module/az.iothub/add-aziothubdevice) command to create a new IoT Edge device in your IoT Hub.

   > Replace `<My Resource Group>` with the name of the resource group that you want to use.  If you do not have a resource group, see [Create resource groups](https://docs.microsoft.com/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups_).  
   >
   > Replace `<My IoT Hub>` with the name of the IoT Hub that you want to use.  If you do not have an IoT Hub, see [Create an IoT Hub](https://docs.microsoft.com/azure/iot-hub/iot-hub-create-through-portal#create-an-iot-hub).

   ```powershell
   Add-AzIoTHubDevice -ResourceGroupName <My Resource Group> -IoTHubName <My IoT Hub> -DeviceId InteropSampleEdgeDev -EdgeEnabled
   ```

   <a name="CreateDownstreamDevice"></a>
1. **Create an IoT Downstream Device**  
   Use the [Add-AzIoTHubDevice](https://docs.microsoft.com/powershell/module/az.iothub/add-aziothubdevice) command to create a downstream device with the IoT Edge device created above as its parent.  

   > Replace `<My Resource Group>` with the name of the resource group that you are using.   
   > Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

   ```powershell
   Add-AzIoTHubDevice -ResourceGroupName <My Resource Group> -IoTHubName <My IoT Hub> -DeviceId InteropSampleDownstreamDev -ParentDeviceId InteropSampleEdgeDev
   ```

   <a name="GetParentConnectionString"></a>
1. **Get Parent device Connection String**  
   Note the connection string for the InteropSampleEdgeDev for use later.

   > Replace `<My Resource Group>` with the name of the resource group that you are using.  
   > Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

   ```powershell
   Get-AzIotHubDCS -ResourceGroupName <My Resource Group> -IotHubName <My IoT Hub> -DeviceId InteropSampleEdgeDev -KeyType primary | fl * 
   ```

   <a name="GetDownstreamConnectionString"></a>
1. **Get Downstream device Connection String**  
   Note the connection string for the InteropSampleDownstreamDev for use later in this sample

   > Replace `<My Resource Group>` with the name of the resource group that you are using. 
   > Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

   ```powershell
   Get-AzIotHubDCS -ResourceGroupName <My Resource Group> -IotHubName <My IoT Hub -DeviceId InteropSampleDownstreamDev -KeyType primary | fl *
   ```
   
   <a name="ContainerRegistry"></a>
1. **Setup Container Registry**
   > Note: If you already have a container registry you can skip this step.  

   Use the [New-AzContainerRegistry](https://docs.microsoft.com/powershell/module/az.containerregistry/New-AzContainerRegistry) command to create a [container registry](https://docs.microsoft.com/en-us/azure/container-registry/) that we will user later in this sample.

   > Replace `<My Resource Group>` with the name of the resource group that you are using. 
   > Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

   ```powershell
   New-AzContainerRegistry -ResourceGroupName <My Resource Group> -Name InteropSampleRegistry -Sku "Basic" -EnableAdminUser
   ```
Go to [Next Step](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
