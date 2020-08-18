# Interop Console App with Linux Edge Module
## Progress

- [x] [Step 1 - Setup Development Environment](./Setup%20Development%20Environment.MD)   
- [x] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [x] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [x] [Step 4 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [x] [Step 5 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [x] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [**Step 7 - Deploy the Modules onto the IoT Edge Device**](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---

# Step 7: Deploy the Modules onto the IoT Edge Device
Having the IoT Edge device set up, we now deploy the Linux modules onto the device.

## Steps in the development VM
Create an IoT Deployment containing the Linux modules and add it to our sample IoT Edge Device:
*   This means to deploy the deployment manifest onto your IoT Edge device
*   Using VS code, follow the steps [here](https://docs.microsoft.com/azure/iot-edge/how-to-deploy-modules-vscode).
 * “Sign in to access your IoT hub”
 * “Deploy to your device” as documented  
 
Select the file 'deployment.amd64.json' in the folder ‘config’ as manifest file to be deployed.

## Steps in the Linux environment of the IoT Edge Device
*   Change into the Linux environment, see description above
*   Run the following command which should list the CSharpModule as depicted in the `screenshot` shown below
```bash
sudo iotedge list
```