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