// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace processingmodule
{
    using System;
    using System.Net.Http;
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

                // Register callback to be called when a message is received by the module
                await ioTHubModuleClient.SetInputMessageHandlerAsync("datainput", ProcessingMessage, ioTHubModuleClient);
            }
            catch (Exception ex)
            {
                Logger.Log($"Init got exception {ex.Message}", LogSeverity.Error);
            }
        }

        /// <summary>
        /// This method will be called whenever a message received by processing module.
        /// </summary>
        static Task<MessageResponse> ProcessingMessage(Message message, object userContext)
        {
            string messageId = message.MessageId == null ? "": message.MessageId;
            var moduleClient = GetClientFromContext(userContext);

            try
            {
                if(message.ContentType == "image/jpeg")
                {
                    Logger.Log($"{UtcDateTime} Received message with message id {messageId} from app");
                    byte[] rawMessageBytes = System.Convert.FromBase64String(Encoding.UTF8.GetString(message.GetBytes()));
                    var processedMessageTask = CallImageClassifier(messageId, rawMessageBytes);
                    var proxyTask = SendMessageToProxyModule(moduleClient, processedMessageTask.Result, messageId, message.Properties["deviceId"]);
                }
                else
                {
                    Logger.Log($"Undefined message content type received.", LogSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"ProcessingMessage got exception {ex.Message}", LogSeverity.Error);
            }
            
            return Task.FromResult(MessageResponse.Completed);
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
        /// This method will send processed message to proxy module for forwarding it to leaf device.
        /// </summary>
        private static async Task SendMessageToProxyModule(ModuleClient moduleClient, string message, string messageId, string deviceId)
        {
            using (var eventMessage = new Message(Encoding.UTF8.GetBytes(message)))
            {
                eventMessage.ContentEncoding = "utf-8";
                eventMessage.ContentType = "application/json";
                eventMessage.MessageId = messageId;
                eventMessage.Properties["deviceId"] = deviceId;
                await moduleClient.SendEventAsync("resultoutput", eventMessage).ConfigureAwait(false);
                Logger.Log($"{UtcDateTime} Sent to proxy module: {messageId}{message}");
            }
        }

        private static string UtcDateTime
        {
            get
            {
                return DateTime.Now.ToUniversalTime().ToString("G");
            }
        }

        /// <summary>
        /// This method will call image classifier for classification of an image received from leaf device.
        /// </summary>
        private static async Task<string> CallImageClassifier(string messageId, byte[] fileContent)
        {
            string message = "";
            try{
                Logger.Log("Invoked CallImageClassifier");

                using(var client = new HttpClient())
                {          
                    using(var request = new HttpRequestMessage())
                    {
                        request.Method = HttpMethod.Post;
                        request.RequestUri = new Uri("http://fruitclassifier/image");
                        request.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                        client.Timeout = TimeSpan.FromSeconds(60);
                        request.Content = new ByteArrayContent(fileContent);
                        Logger.Log($"{UtcDateTime} Request to classifier: {messageId}");
                        var response = await client.SendAsync(request);
                        message = await response.Content.ReadAsStringAsync();
                        Logger.Log($"{UtcDateTime} Response from classifier: {messageId}{message}");
                    }
                }
            }
             catch (Exception ex)
            {
                Logger.Log($"CallImageClassifier got exception {ex.Message}", LogSeverity.Error);
                message = $"CallImageClassifier got exception {ex.Message}";
            }
            return message;
        }
    }
}
