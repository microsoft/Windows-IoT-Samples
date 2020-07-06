namespace CSharpModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Newtonsoft.Json;

    class Program
    {
        private static string UtcDateTime
        {
            get
            {
                return DateTime.Now.ToUniversalTime().ToString("G");
            }
        }

                 /// <summary>
        /// This method will send message to leaf device by invoking leaf device direct method.
        /// </summary>
        static async Task SendMessageToLeafDevice(string deviceId, ModuleClient moduleClient )
        {
            try
            {
                string message = $"{UtcDateTime} hello from edge!";
                string jString = JsonConvert.SerializeObject(message);
                var methodRequest = new MethodRequest("LeafDeviceDirectMethod", Encoding.UTF8.GetBytes(jString));
                var response = await moduleClient.InvokeMethodAsync(deviceId, methodRequest);
                if(response.Status == 200)
                {
                    Console.WriteLine($"{UtcDateTime} message {message} sent");
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

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
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
                    await SendMessageToLeafDevice(message.ConnectionDeviceId, moduleClient);
                    }catch(Exception ex)
                    {
                        Console.WriteLine($"Exception occurred\n stacktrace{ex.StackTrace}");
                    }
                    return MessageResponse.Completed;
        }
    }
}
