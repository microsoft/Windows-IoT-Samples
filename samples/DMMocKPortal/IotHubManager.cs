// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DMMockPortal
{
    public struct DeviceMethodReturnValue
    {
        public int Status;
        public string Payload;
    }

    public struct DeviceData
    {
        public string deviceJson;
        public string tagsJson;
        public string reportedPropertiesJson;
        public string desiredPropertiesJson;
    }

    class IoTHubManager
    {
        public const int DirectMethodSuccessCode = 200;

        static private string messageDeviceTwinFunctionalityNotFound = "Device Twin functionality not found." + Environment.NewLine + "Make sure you are using the latest Microsoft.Azure.Devices package.";

        public IoTHubManager(string iotHubConnectionString)
        {
            _iotHubConnectionString = iotHubConnectionString;
        }

        public async Task UpdateDesiredProperties(string deviceId, string updateJson /* string name, object value*/)
        {
            Debug.WriteLine("updateJson: " + updateJson);

            dynamic registryManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
            if (registryManager != null)
            {
                try
                {
                    string assemblyClassName = "Twin";
                    Type typeFound = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from assemblyType in assembly.GetTypes()
                                      where assemblyType.Name == assemblyClassName
                                      select assemblyType).FirstOrDefault();

                    if (typeFound != null)
                    {
                        var deviceTwin = await registryManager.GetTwinAsync(deviceId);

                        dynamic dp = JsonConvert.DeserializeObject(updateJson, typeFound);
                        dp.DeviceId = deviceId;
                        dp.ETag = deviceTwin.ETag;
                        registryManager.UpdateTwinAsync(dp.DeviceId, dp, dp.ETag);
                    }
                    else
                    {
                        MessageBox.Show(messageDeviceTwinFunctionalityNotFound, "Device Twin Properties Update");
                    }
                }
                catch (Exception ex)
                {
                    string errMess = "Update Twin failed. Exception: " + ex.ToString();
                    MessageBox.Show(errMess, "Device Twin Desired Properties Update");
                }
            }
            else
            {
                MessageBox.Show("Registry Manager is no initialized!", "Device Twin Desired Properties Update");
            }
        }

        public async Task<DeviceMethodReturnValue> InvokeDirectMethod(string deviceId, string methodName, string methodPayload)
        {
            TimeSpan timeoutInSeconds = new TimeSpan(0, 0, 30);
            CancellationToken cancellationToken = new CancellationToken();

            DeviceMethodReturnValue deviceMethodReturnValue;
            deviceMethodReturnValue.Status = DirectMethodSuccessCode;
            deviceMethodReturnValue.Payload = "";

            var serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);
            try
            {
                var cloudToDeviceMethod = new CloudToDeviceMethod(methodName, timeoutInSeconds);
                cloudToDeviceMethod.SetPayloadJson(methodPayload);

                var result = await serviceClient.InvokeDeviceMethodAsync(deviceId, cloudToDeviceMethod, cancellationToken);

                deviceMethodReturnValue.Status = result.Status;
                deviceMethodReturnValue.Payload = result.GetPayloadAsJson();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Device Twin Properties");
            }

            return deviceMethodReturnValue;
        }

        public async Task<DeviceData> GetDeviceData(string deviceId)
        {
            DeviceData result = new DeviceData();

            dynamic registryManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
            try
            {
                var deviceTwin = await registryManager.GetTwinAsync(deviceId);
                if (deviceTwin != null)
                {
                    result.deviceJson = deviceTwin.ToJson();
                    result.tagsJson = deviceTwin.Tags.ToJson();
                    result.reportedPropertiesJson = deviceTwin.Properties.Reported.ToJson();
                    result.desiredPropertiesJson = deviceTwin.Properties.Desired.ToJson();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + "Make sure you are using the latest Microsoft.Azure.Devices package.", "Device Twin Properties");
            }
            return result;
        }

        private string _iotHubConnectionString;
    }
}
