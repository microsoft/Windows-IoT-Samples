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
                "Certificate with root CA in PEM format",
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
