// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DMMockPortal
{
    class DeviceSummary
    {
        public string Name { get; set; }
        public string FailedCount { get; set; }
        public string PendingCount { get; set; }
    }

    class DevicesQuery
    {
        public uint DeviceCount { get; set; }
        public uint FailedDeviceCount { get; set; }
        public uint PendingDeviceCount { get; set; }
        public uint SuccessDeviceCount { get; set; }
        public string TargetCondition { get; set; }
        public Dictionary<string, DeviceSummary> Devices { get; set; }

        public DevicesQuery(string targetConditionOrQuery)
        {
            string s = targetConditionOrQuery.ToLower();

            int index = s.IndexOf(JsonTemplates.Where);
            if (index != -1)
            {
                TargetCondition = targetConditionOrQuery.Substring(index + JsonTemplates.Where.Length);
            }
            else
            {
                TargetCondition = targetConditionOrQuery;
            }

            Devices = new Dictionary<string, DeviceSummary>();
        }

        public async Task Refresh(string connectionString)
        {
            // Clear
            DeviceCount = 0;
            FailedDeviceCount = 0;
            PendingDeviceCount = 0;
            SuccessDeviceCount = 0;

            Devices.Clear();

            // Query
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonTemplates.DevicesQuery);

            if (!String.IsNullOrEmpty(TargetCondition))
            {
                sb.Append(JsonTemplates.Where + " " + TargetCondition);
            }

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            IQuery query = registryManager.CreateQuery(sb.ToString());
            IEnumerable<string> results = await query.GetNextAsJsonAsync();

            // Parse
            foreach (string s in results)
            {
                JObject jObject = (JObject)JsonConvert.DeserializeObject(s);

                ++DeviceCount;

                long failedCount = 0;
                if (jObject.ContainsKey(JsonTemplates.FailedCount))
                {
                    failedCount = (long)jObject[JsonTemplates.FailedCount];
                }

                if (failedCount != 0)
                {
                    ++FailedDeviceCount;
                }

                long pendingCount = 0;
                if (jObject.ContainsKey(JsonTemplates.PendingCount))
                {
                    pendingCount = (long)jObject[JsonTemplates.PendingCount];
                }

                if (pendingCount != 0)
                {
                    ++PendingDeviceCount;
                }

                string deviceId = "<unkown>";
                if (jObject.ContainsKey(JsonTemplates.DeviceId))
                {
                    deviceId = (string)jObject[JsonTemplates.DeviceId];
                }

                DeviceSummary ds = new DeviceSummary();
                ds.Name = deviceId;
                ds.FailedCount = failedCount.ToString();
                ds.PendingCount = pendingCount.ToString();

                Devices[ds.Name] = ds;
            }

            SuccessDeviceCount = DeviceCount - FailedDeviceCount;
        }
    }
}
