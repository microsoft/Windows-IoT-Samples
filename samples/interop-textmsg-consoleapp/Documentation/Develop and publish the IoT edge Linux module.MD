# Interop Console App with Linux Edge Module
## Progress

- [x] [Step 1 - Setup Development Environment](./Setup%20Development%20Environment.MD)   
- [x] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [ ] [**Step 3 - Develop and publish the IoT Edge Linux module**](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [ ] [Step 4 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] [Step 5 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
# Step 3: Develop and publish the IoT edge Linux module
In this part we develop and publish the Linux modules in the development VM using Visual Studio Code.

[1. Create a new IoT Edge module project for C#](step1)  
[2. Update Program.cs](step2)  
[3. Update deployment.template.json](step3)  
[4. Create .env file](step4)  
[5. Update module.json](step5)  
[6. Build and push your module](step6)   

---
<a name="step1"></a>
## 1. Create a new IoT Edge module project for C#
Perform the following steps from `Tutorial: Develop a C# IoT Edge module for Linux devices`
* [Prerequisites](https://docs.microsoft.com/azure/iot-edge/tutorial-csharp-module#prerequisites)
* [Create a new project](https://docs.microsoft.com/azure/iot-edge/tutorial-csharp-module#create-a-new-project)
---
<a name="step2"></a>
## 2. Update Program.cs
Open the file `Program.cs` from folder `modules/CSharpModule` in the Visual Studio Code explorer. 

* Add the following line to the top of the `CSharpModule` namespace.
```chsarp
using Newtonsoft.Json
```

* Add the following `UtcDateTime` utility method to the class `Program`.
```chsarp
        private static string UtcDateTime
        {
            get
            {
                return DateTime.Now.ToUniversalTime().ToString("G");
            }
        }
```

* Add the `SendMessageToLeafDevice` method to the class `Program`. 

```csharp
         /// <summary>
        /// This method uses 'moduleClient.InvokeMethodAsync to send messages to leaf device.
        /// </summary>
        static async Task SendMessageToLeafDevice(string deviceId, ModuleClient moduleClient )
        {
            try
            {
                string message = $"hello from edge!";
                string jString = JsonConvert.SerializeObject(message);
                var methodRequest = new MethodRequest("LeafDeviceDirectMethod", Encoding.UTF8.GetBytes(jString));
                var response = await moduleClient.InvokeMethodAsync(deviceId, methodRequest);
                if(response.Status == 200)
                {
                    Console.WriteLine($"{UtcDateTime} sent message:{message}");
                }
                else
                {
                    Console.WriteLine($"Error occurred invoking LeafDeviceDirectMethod. error status code {response.Status}");
                }
   	        }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessageToLeafDevice got exception {ex.Message}");
            }
        }

```

* Replace the `PipeMessage` method with `LeafDeviceMessageHandlerAsync` methiod which will be invoked upon messages received from Windows application.
```csharp
         /// <summary>
        /// This method is called whenever a leaf device sent a message to edge. 
        /// </summary>
        static async Task<MessageResponse> LeafDeviceMessageHandlerAsync(Message message, object userContext)
        {
	       try{
                    var moduleClient = userContext as ModuleClient;
                    if (moduleClient == null)
                    {
                        throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
                    }
                    var messageData = message.GetBytes();
                    string receivedMessage = Encoding.UTF8.GetString(messageData);
                    Console.WriteLine($"Received message: {receivedMessage}");
                    SendMessageToLeafDevice(message.ConnectionDeviceId, moduleClient);
                    }catch(Exception ex)
	       {
                      Console.WriteLine("Exception occurred {ex.StackTrace}");
                     }
                    return MessageResponse.Completed;
        }

```

* Replace the Init function with the following:

> Find the Init task function in Program.cs. This function creates and configures a ModuleClient object, which allows the module to connect to the local Azure IoT Edge runtime to send and receive messages. After creating the ModuleClient, the code sets message handler to read the messages from downstream device. To make this change, replace the Init function with the below code.
```csharp
         /// Summary
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("ModuleClient initialized");

            // Register callback to be called when a message is received by the proxy module from leaf device.
            await ioTHubModuleClient.SetInputMessageHandlerAsync("leafdeviceinput", LeafDeviceMessageHandlerAsync, ioTHubModuleClient);
        }

```
---
<a name="step3"></a>
## 3. Update `deployment.template.json`

* Configure the routing table in the project file deployment.template.json to redirect the messages received from the windows application to CSharpModule by removing all the entries in the routes and adding below entry to “routes”.
```json
"leafdeviceinputendpoint": "FROM /messages/* WHERE NOT IS_DEFINED($connectionModuleId) INTO BrokeredEndpoint(\"/modules/CSharpModule/inputs/leafdeviceinput\")"
```

* Remove below temperature sensor module from deployment.template.json as it is not needed.
```json
,
          "SimulatedTemperatureSensor": {
            "version": "1.0",
            "type": "docker",
            "status": "running",
            "restartPolicy": "always",
            "settings": {
              "image": "mcr.microsoft.com/azureiotedge-simulated-temperature-sensor:1.0",
              "createOptions": {}
            }
}

```

* Add your registry credentials to the deployment.template.json file, setting <registryname> to the name of your container registry:
```json
<registryname>": { "username": "$CONTAINER_REGISTRY_USERNAME_<registryname>", "password": "$CONTAINER_REGISTRY_PASSWORD_<registryname>", "address": "<registryname>.azurecr.io" }
```
---
<a name="step4"></a>
## 4. Create `.env` file
Create a .env file in the main project folder with the container registry authentication details as variables:
```txt
CONTAINER_REGISTRY_USERNAME_<registryname>=XXX CONTAINER_REGISTRY_PASSWORD_<registryname>=XXX
```
Replace the <registryname> with the name of your container registry and the two 'XXX' in with the username and password for that container registry as found in the Azure Portal (navigate to your container registry in the Azure Portal and go to the tab 'Access Keys').  

---  
<a name="step5"></a>
## 5. Update `module.json`
Ensure your registry name is set in the modules/CSharpModule/module.json (see former “Create new project” step):
```json
… "repository": "<registryname>.azurecr.io" …
```
---
<a name="step6"></a>
## 6. Build and push your module

Perform the following steps from `Tutorial: Develop a C# IoT Edge module for Linux devices`
* [Build and push your module](https://docs.microsoft.com/azure/iot-edge/tutorial-csharp-module#build-and-push-your-module)

Use the container registry credentials of your container registry for the docker login.

Go to [Next Step](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
