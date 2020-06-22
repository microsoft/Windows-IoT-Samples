// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace proxymodule
{
    using System;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    class Program
    {

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
        /// messages.
        /// </summary>
        static async Task Init()
        {
            try
            {
                AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
                ITransportSettings[] settings = { amqpSetting };

                // Open a connection to the Edge runtime
                ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
                await ioTHubModuleClient.OpenAsync();

                // Register callback to be called when a message is received by the proxy module from leaf device.
                await ioTHubModuleClient.SetInputMessageHandlerAsync("leafdeviceinput", PipeMessage, ioTHubModuleClient);

                // Register callback to be called when a message is received by the proxy module from processing module.
                await ioTHubModuleClient.SetInputMessageHandlerAsync("resultinput", ForwardMessageToLeafDevice, ioTHubModuleClient);
            }
            catch (Exception ex)
            {
                Logger.Log($"Init got exception {ex.Message}", LogSeverity.Error);
            }
        }

        /// <summary>
        /// This method is called whenever a leaf device sent a message to edge. 
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            var moduleClient = GetClientFromContext(userContext);
            if(message.ContentType == "application/json")
            {
                return await ForwardMessageToProcessingModule(message, moduleClient).ConfigureAwait(false);
            }
            else
            {
                return await SendMessageToCloud(message, moduleClient).ConfigureAwait(false);
            }
        }

        static ModuleClient GetClientFromContext(object userContext)
        {
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new ArgumentException($"Could not cast userContext. Expected {typeof(ModuleClient)} but got: {userContext.GetType()}");
            }
            return moduleClient;
        }

        /// <summary>
        /// This method is called whenever a processing module send message for forwarding to leaf device.
        /// It log the message, and forward message to leaf device.
        /// </summary>
        static async Task<MessageResponse> ForwardMessageToLeafDevice(Message message, object userContext)
        {
            try
            {
                var moduleClient = GetClientFromContext(userContext);
                string deviceId = message.Properties["deviceId"];
                var messageData = message.GetBytes();
                string receivedMessage = Encoding.UTF8.GetString(messageData);
                string jString = JsonConvert.SerializeObject(receivedMessage);
                Logger.Log($"{UtcDateTime} ForwardMessageToLeafDevice: Received message={receivedMessage}.");
                var methodRequest = new MethodRequest("LeafDeviceDirectMethod", Encoding.UTF8.GetBytes(jString));
                var response = await moduleClient.InvokeMethodAsync(deviceId, methodRequest);
                if(response.Status == 200)
                {
                    Logger.Log($"{UtcDateTime} forwarded to leaf device.");
                }
                else
                {
                    Logger.Log($"Error occurred invoking LeafDeviceDirectMethod. error status code {response.Status}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"ForwardMessageToLeafDevice got exception {ex.Message}", LogSeverity.Error);
            }

            return MessageResponse.Completed; 
        }

        /// <summary>
        /// This method is called whenever a proxy module received message from leaf device to forward to processing module.
        /// It log the message, and forward message to processing module.
        /// </summary>
        private static async Task<MessageResponse> ForwardMessageToProcessingModule(Message messageFwd, ModuleClient moduleClient)
        {
            using(var message = new Message(messageFwd.GetBytes()))
            {
                string messageId = messageFwd.MessageId == null ? "": messageFwd.MessageId;
                string messageString = Encoding.UTF8.GetString(message.GetBytes());
                message.Properties.Add("deviceId", messageFwd.ConnectionDeviceId);
                message.MessageId = messageId;
                message.ContentEncoding = messageFwd.ContentEncoding;
                message.ContentType = messageFwd.ContentType;
                Logger.Log($"{UtcDateTime} Received message {messageString} from app: {messageId}");
                await moduleClient
                .SendEventAsync("processingoutput", message)
                .ConfigureAwait(false);
            }

            return MessageResponse.Completed;
        }

        /// <summary>
        /// Send message to cloud.
        /// It log the message, and forward message to cloud.
        /// </summary>
        private static async Task<MessageResponse> SendMessageToCloud(Message message, ModuleClient moduleClient)
        {
            string messageString = Encoding.UTF8.GetString(message.GetBytes());
            Logger.Log($"{UtcDateTime} Message contenttype is not application/json, message={messageString} sent to cloud.");
            message.Properties.Add("DeadLetter", "true");
            await moduleClient
                .SendEventAsync("cloudmessage", message)
                .ConfigureAwait(false);

            return MessageResponse.Completed;
        }

        private static string UtcDateTime
        {
            get
            {
                return DateTime.Now.ToUniversalTime().ToString("G");
            }
        }
    }
}
