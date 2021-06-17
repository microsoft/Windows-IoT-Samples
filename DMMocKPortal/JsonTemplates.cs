// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;

namespace DMMockPortal
{
    static class JsonTemplates
    {
        public static readonly string DesiredProperties;
        public static readonly string Tags;
        public static readonly string SuccessQueryName = "successQuery";
        public static readonly string SuccessQueryValue = "SELECT deviceId FROM devices WHERE properties.reported.apps.toaster.__meta.state='failed'";
        public static readonly string DevicesQuery;
        public static readonly string FailedCount = "failedCount";
        public static readonly string PendingCount = "pendingCount";
        public static readonly string DeviceId = "deviceId";
        public static readonly string Where = "where";
        public static readonly string Summary = "__summary";
        public static readonly string Meta = "__meta";
        public static readonly string DeploymentId = "deploymentId";
        public static readonly string DeploymentIdUnspecified = "unspecified";

        static JsonTemplates()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \n");
            sb.Append("  \"windowsUpdate\": {\n");
            sb.Append("    \"autoUpdate\": 1\n");
            sb.Append("  }\n");
            sb.Append("}\n");
            DesiredProperties = sb.ToString();

            sb.Clear();
            sb.Append("{ \n");
            sb.Append("  \"tags\": {\n");
            sb.Append("    \"location\": \"North America\"\n");
            sb.Append("  }\n");
            sb.Append("}\n");
            Tags = sb.ToString();

            sb.Clear();
            sb.Append("SELECT deviceId, ");
            sb.Append("       properties.reported.__summary.failedCount, ");
            sb.Append("       properties.reported.__summary.pendingCount ");
            sb.Append("FROM Devices ");
            DevicesQuery = sb.ToString();
        }
    }
}
