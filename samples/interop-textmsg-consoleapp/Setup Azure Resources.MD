# Step 2: Set up Azure resources  
This part describes how to set up the required Azure entities with the [Azure Cloud Shell](https://docs.microsoft.com/azure/cloud-shell/overview). Alternatively you can use [Azure PowerShell](https://docs.microsoft.com/powershell/azure/) module. This should be executed on any Windows host other than the (emulated) target device. We propose using the VM that serves as development environment. 

[1. Start your PowerShell session](step1)  
[2. Determine Azure subscription Id](step2)  
[3. Set the subscription Id context](step3)  
[4. Create an IoT Edge device](step4)  
[5. Create an IoT Downstream Device](step5)  
[6. Get Parent device Connection String](step6)  
[7. Get Downstream device Connection String](step7)  
[8. Setup Container Registry](step8)  

----
<a name="step1"></a>
## 1. Start your PowerShell session

* To use [Azure Cloud Shell](https://docs.microsoft.com/azure/cloud-shell/quickstart-powershell) see [Start Cloud Shell](https://docs.microsoft.com/azure/cloud-shell/quickstart-powershell#start-cloud-shell) instructions
  

* To use [Azure PowerShell](https://docs.microsoft.com/powershell/azure/), load the Az PowerShell module  
     * Click Start, type `PowerShell`  
     * Right-click Windows PowerShell, then click Run as administrator.
     * Set PowerShell execution policy with `powershell -ExecutionPolicy Bypass`
     * Import the Az module with `Import-Module Az`
     * Sign in to the Azure Portal with `Connect-AzAccount`
---
<a name="step2"></a>
## 2. Determine Azure subscription Id
Use [Get-AzSubscription](https://docs.microsoft.com/powershell/module/az.accounts/get-azsubscription?view=azps-4.5.0) to list the subscriptions associated with your account then copy the value in the **Id** column associated with the Name of your subscription
```powershell
Get-AzSubscription
```
---
<a name="step3"></a>
## 3. Set the subscription Id context
Use the [Set-AzContext](https://docs.microsoft.com/powershell/module/az.accounts/set-azcontext) command to set the subscription ID context.  
> * Replace **Id** with the Id copied from above.
```powershell
Set-AzContext -SubscriptionId "Id"
```
---
<a name="step4"></a>
## 4. Create an IoT Edge Device
Use the [Add-AzIoTHubDevice](https://docs.microsoft.com/powershell/module/az.iothub/add-aziothubdevice) command to create a new IoT Edge device in your IoT Hub.

> * Replace `<My Resource Group>` with the name of the resource group that you want to use.  If you do not have a resource group, see [Create resource groups](https://docs.microsoft.com/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups_).
> * Replace `<My IoT Hub>` with the name of the IoT Hub that you want to use.  If you do not have an IoT Hub, see [Create an IoT Hub](https://docs.microsoft.com/azure/iot-hub/iot-hub-create-through-portal#create-an-iot-hub).

```powershell
Add-AzIoTHubDevice -ResourceGroupName <My Resource Group> -IoTHubName <My IoT Hub> -DeviceId InteropSampleEdgeDev -EdgeEnabled
```
---
<a name="step5"></a>
## 5. Create an IoT Downstream Device.
Use the [Add-AzIoTHubDevice](https://docs.microsoft.com/powershell/module/az.iothub/add-aziothubdevice) command to create a downstream device with the IoT Edge device created above as its parent.  

> * Replace `<My Resource Group>` with the name of the resource group that you are using. 
> * Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

```powershell
Add-AzIoTHubDevice -ResourceGroupName <My Resource Group> -IoTHubName <My IoT Hub> -DeviceId InteropSampleDownstreamDev -ParentDeviceId InteropSampleEdgeDev
```
---
<a name="step6"></a>
## 6. Get Parent device Connection String
Note the connection string for the InteropSampleEdgeDev for use later.

> * Replace `<My Resource Group>` with the name of the resource group that you are using. 
> * Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

```powershell
Get-AzIotHubDCS -ResourceGroupName <My Resource Group> -IotHubName <My IoT Hub> -DeviceId InteropSampleEdgeDev -KeyType primary | fl * 
```
---
<a name="step7"></a>
## 7. Get Downstream device Connection String
Note the connection string for the InteropSampleDownstreamDev for use later in this sample

> * Replace `<My Resource Group>` with the name of the resource group that you are using. 
> * Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

```powershell
Get-AzIotHubDCS -ResourceGroupName <My Resource Group> -IotHubName <My IoT Hub -DeviceId InteropSampleDownstreamDev -KeyType primary | fl *
```
---
<a name="step8"></a>
## 8. Setup Container Registry
> Note: If you already have a container registry you can skip this step.  

Use the [New-AzContainerRegistry](https://docs.microsoft.com/powershell/module/az.containerregistry/New-AzContainerRegistry) command to create a [container registry](https://docs.microsoft.com/en-us/azure/container-registry/) that we will user later in this sample.

> * Replace `<My Resource Group>` with the name of the resource group that you are using. 
> * Replace `<My IoT Hub>` with the name of the IoT Hub that you are using.  

```powershell
New-AzContainerRegistry -ResourceGroupName <My Resource Group> -Name InteropSampleRegistry -Sku "Basic" -EnableAdminUser
```
