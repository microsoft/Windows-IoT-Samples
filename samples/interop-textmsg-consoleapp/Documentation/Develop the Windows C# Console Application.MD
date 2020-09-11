# Interop Console App with Linux Edge Module
## Progress

- [x] [Introduction](../README.md)  
- [x] [Step 1 - Setup Development Environment](./Setup%20DevVM.MD)   
- [x] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [x] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [x] [Step 4 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [ ] **Step 5 - Develop the Windows C# Console Application**  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
# Step 5: Develop the Windows C# Console Application
This part describes how to develop the Windows console application in the development VM.

1. In VS Code Explorer, open the EdgeSolution folder created earlier.

1. Open the VS Code integrated terminal by selecting **View** > **Terminal**.

1. Run below command to create project.  
   ```powershell
   dotnet new console -n LeafDeviceApp
   ```
1. Create the file “EdgeSolution\LeafDeviceApp\CertificateManager.cs” with the following code
       ```csharp
       using System;
       using System.IO;
       using System.Security.Cryptography.X509Certificates;

       namespace LeafDeviceApp
       {
           internal class CertificateManager
           {
               /// <summary>
               /// Add certificate in local cert store for use by downstream device
            /// client for secure connection to IoT Edge runtime.
            ///
            ///    Note: On Windows machines, if you have not run this from an Administrator prompt,
            ///    a prompt will likely come up to confirm the installation of the certificate.
            ///    This usually happens the first time a certificate will be installed.
            /// </summary>
            public static void InstallCACert(string certificatePath)
            {
                if (string.IsNullOrWhiteSpace(certificatePath))
                {
                    throw new ArgumentNullException(nameof(certificatePath));
                }

                Console.WriteLine($"User configured CA certificate path: {certificatePath}");
                if (!File.Exists(certificatePath))
                {
                    // cannot proceed further without a proper cert file
                    Console.WriteLine($"Invalid certificate file: {certificatePath}");
                    throw new InvalidOperationException("Invalid certificate file.");
                }
                else
                {
                    Console.WriteLine($"Attempting to install CA certificate: {certificatePath}");
                    var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(certificatePath)));
                    Console.WriteLine($"Successfully added certificate: {certificatePath}");
                    store.Close();
                }
            }
        }
       }
       ```

1. Replace the EdgeSolution\LeafDeviceApp\Program.cs file with the below code which includes:

    * Sending message to iotedge device using [DeviceClient.SentEventAsync](https://docs.microsoft.com/dotnet/api/microsoft.azure.devices.client.deviceclient.sendeventasync)
    * Initialize DeviceClient object by using [DeviceClient.CreateFromConnectionString](https://docs.microsoft.com/dotnet/api/microsoft.azure.devices.client.deviceclient.sendeventasync) method.
    * Create message object and invoke [DeviceClient.SendEventAsync](https://docs.microsoft.com/dotnet/api/microsoft.azure.devices.client.deviceclient.sendeventasync) method to to pass the message.
    * Define and register message handler using the [DeviceClient.SetMethodHandlerAsync](https://docs.microsoft.com/dotnet/api/microsoft.azure.devices.client.deviceclient.setmethoddefaulthandlerasync) method.
    * Options to read IoT hub connection string, IoT edge device hostname, edge root certificate file path.


    ```csharp
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.CommandLineUtils;
    using Newtonsoft.Json;

    namespace LeafDeviceApp
    {
        class Program
        {
            static void Main(string[] args)
            {
                InitializeApp();

                app.OnExecute(() =>
                {
                    InstallCertificate();

                    string iotHubConnectionString = GetConnectionString();
                    string gatewayHost = GetGatewayHostName();
                    string deviceConnectionString = $"{iotHubConnectionString};GatewayHostName={gatewayHost}";
                    deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);
                    deviceClient.SetMethodHandlerAsync("LeafDeviceDirectMethod", LeafDeviceMethodCallback, null).Wait();

                    ConsoleKeyInfo input;
                    while(true)
                    {
                        Console.WriteLine("Press 1 to send message and any other key for exit.");
                        input = Console.ReadKey();
                        if(input.Key != ConsoleKey.D1)
                        {
                            break;
                        }
                        SendMessage().Wait();
                    }
                    return 0;
                });

                app.Execute(args);
                Console.ReadLine();
            }

            private static CommandLineApplication app = new CommandLineApplication();
            private static CommandOption connectionStringOption;
            private static CommandOption certificateOption;
            private static CommandOption gatewayHostNameOption;
            private static DeviceClient deviceClient;

            /// <summary>
            /// Initializes the instance of the CommandLineApplication
            /// </summary>
            private static void InitializeApp()
            {
                app.Name = "LeafDeviceApp";
                app.Description = "Leaf device to communicate with edge gateway device.";

                app.HelpOption("-?|-h|--help");

                connectionStringOption = app.Option(
                    "-x|--connection",
                    @"IoT Hub Connection String e.g HostName=hubname.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xxxxxx;",
                    CommandOptionType.SingleValue);

                certificateOption = app.Option(
                    "-c|--certificate",
                    "File path to Root CA certificate in PEM format",
                    CommandOptionType.SingleValue);

                gatewayHostNameOption = app.Option(
                    "-g|--gateway-host-name",
                    "Fully qualified domain name of the edge device acting as a gateway. e.g. iotedge-xxx.westus2.cloudapp.azure.com ",
                    CommandOptionType.SingleValue);
            }

            /// <summary>
            /// Looks for a certificate either passed as a parameter or in the CA_CERTIFICATE_PATH
            /// environment variable and, if present, attempts to install the certificate
            /// </summary>
            private static void InstallCertificate()
            {
                string certificatePath;
                if (!certificateOption.HasValue())
                {
                    certificatePath = Environment.GetEnvironmentVariable("CA_CERTIFICATE_PATH");
                }
                else
                {
                    certificatePath = certificateOption.Value();
                }

                if (!String.IsNullOrWhiteSpace(certificatePath))
                {
                    CertificateManager.InstallCACert(certificatePath);
                }
            }

            /// <summary>
            /// Retrieves the value of the connection string from the connectionStringOption. 
            /// If the connection string wasn't passed method prompts for the connection string.
            /// </summary>
            /// <returns></returns>
            private static string GetConnectionString()
            {
                string connectionString;

                if (!connectionStringOption.HasValue())
                {
                    connectionString = Environment.GetEnvironmentVariable("DEVICE_CONNECTION_STRING");
                    app.ShowHint();
                }
                else
                {
                    connectionString = connectionStringOption.Value();
                }

                while (String.IsNullOrWhiteSpace(connectionString))
                {
                    Console.WriteLine("Please enter IoT Hub Connection String:");
                    connectionString = Console.ReadLine();
                }

                Console.WriteLine($"Using connection string: {connectionString}");
                return connectionString;
            }

            /// <summary>
            /// Get fully qualified domain name of the edge device acting as a gateway. e.g. iotedge-xxx.westus2.cloudapp.azure.com.
            /// If the gateway host name wasn't passed method prompts for the gateway host name.
            /// </summary>
            /// <returns></returns>
            private static string GetGatewayHostName()
            {
                string gatewayHostName;

                if (!gatewayHostNameOption.HasValue())
                {
                    gatewayHostName = Environment.GetEnvironmentVariable("GATEWAY_HOST_NAME");
                    app.ShowHint();
                }
                else
                {
                    gatewayHostName = gatewayHostNameOption.Value();
                }

                while (String.IsNullOrWhiteSpace(gatewayHostName))
                {
                    Console.WriteLine("Please enter edge gate way host name:");
                    gatewayHostName = Console.ReadLine();
                }

                Console.WriteLine($"Using gateway host name: {gatewayHostName}");
                return gatewayHostName;
            }

            private static Task<MethodResponse> LeafDeviceMethodCallback(MethodRequest methodRequest, object userContext )
            {
                if (methodRequest.Data != null)
                {
                    var data = Encoding.UTF8.GetString(methodRequest.Data);
                    Console.WriteLine($"Edge reply: {data}");
                    string jString = JsonConvert.SerializeObject("Success");
                    return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(jString), 200));
                }
                else
                {
                    Console.WriteLine("Edge reply: Empty");
                    string jString = JsonConvert.SerializeObject("Empty");
                    return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(jString), 400));
                }
            }

            /// <summary>
            /// This method will send message to edge gateway/iothub.
            /// </summary>
            private static async Task SendMessage()
            {
                string message = $"Hi Edge, How are you doing!";
                Console.WriteLine($"Device says: {message}");
                using (var eventMessage = new Message(Encoding.UTF8.GetBytes(message)))
                {
                    // Set the content type and encoding so the IoT Hub knows to treat the message body as JSON
                    eventMessage.ContentEncoding = "utf-8";
                    eventMessage.ContentType = "application/json";
                    await deviceClient.SendEventAsync(eventMessage);                
                }
            }
        }
    }
    ```


1. In the VS Code explorer, open **EdgeSolution** > **LeafDeviceApp** > **LeafDeviceApp.csproj** and add below entry inside the Project node:  
    ```
    <ItemGroup>
        <PackageReference Include="Microsoft.Azure.Devices.Client" Version="1.18.1" />
        <PackageReference Include="Microsoft.Extensions.CommandLineUtils" Version="1.1.1" /> 
    </ItemGroup>
    ```

1. Open the VS Code integrated terminal by selecting **View** > **Terminal** then, change to the folder EdgeSolution\LeafDeviceApp\ and run below command for building the project.

   ```powershell
   dotnet build
   ```

1. Publish your binary a [trim self-contained executable](https://docs.microsoft.com/dotnet/core/deploying/trim-self-contained) using the following command: 

   ```powershell
   dotnet publish -c Release -r win10-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
   ```
   It will create a single binary along with the symbols under EdgeSolution\LeafDeviceApp\bin\Release\netcoreapp3.1\win10-x64\publish\.  

1. Copy the following build artifact folder onto the IoT device’s Windows OS:
   ```powershell
   EdgeSolution\LeafDeviceApp\bin\Release\netcoreapp3.1\win10-x64\publish\
   ```

Go to [Next Step](./Configuring%20the%20IoT%20Edge%20Device.MD)  
