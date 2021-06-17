// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DMMockPortal
{
    public delegate void SelectedDeploymentChangedEventType(string deploymentId);

    public partial class DeviceListControl : UserControl
    {
        public DeploymentListControl DeploymentListPanel { get; set; }

        public SelectedDeploymentChangedEventType SelectedReportedDeploymentChangedEvent;

        public DeviceListControl()
        {
            InitializeComponent();

            DeviceTwinPanel.ApplyPropertiesEvent += OnApplyPropertiesEvent;
            DeviceTwinPanel.SelectedDeploymentChangedEvent += OnSelectedReportedDeploymentChanged;
        }

        public void SetConnectionString(string cs)
        {
            _connectionString = cs;
            DeviceTwinPanel.SetConnectionString(cs);
        }

        private async void OnApplyPropertiesEvent(string propertiesJsonString)
        {
            await ApplyProperties(propertiesJsonString);
        }

        private void OnSelectedReportedDeploymentChanged(string deploymentId)
        {
            SelectedDeploymentChangedEventType selectedDeploymentChanged = SelectedReportedDeploymentChangedEvent;
            if (selectedDeploymentChanged != null)
            {
                selectedDeploymentChanged(deploymentId);
            }
        }

        private void OnFilterHasErrors(object sender, RoutedEventArgs e)
        {
            RebuildDeviceList();
        }

        private void OnFilterHasPending(object sender, RoutedEventArgs e)
        {
            RebuildDeviceList();
        }

        private void RebuildDeviceList()
        {
            DevicesList.Items.Clear();
            foreach (var pair in _devices)
            {
                if (FilterHasErrorsCheckBox.IsChecked == true && pair.Value.FailedCount == "0")
                {
                    continue;
                }

                if (FilterHasPendingCheckBox.IsChecked == true && pair.Value.PendingCount == "0")
                {
                    continue;
                }
                DevicesList.Items.Add(pair.Value);
            }
        }

        public async void LoadDeploymentDevicesAsync(string targetQuery)
        {
            DevicesQuery allDevicesQuery = new DevicesQuery(targetQuery);
            await allDevicesQuery.Refresh(_connectionString);
            _devices = allDevicesQuery.Devices;

            RebuildDeviceList();
        }

        public async Task ApplyProperties(string propertiesJsonString)
        {
            if (DevicesList.SelectedItems.Count == -1)
            {
                MessageBox.Show("Select at least one device!");
                return;
            }

            StringBuilder sb = new StringBuilder();

            IoTHubManager iotHubManager = new IoTHubManager(_connectionString);
            foreach (DeviceSummary device in DevicesList.SelectedItems)
            {
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }
                sb.Append(device.Name);
                Debug.WriteLine("Applying desired properties to: " + device.Name);
                await iotHubManager.UpdateDesiredProperties(device.Name, propertiesJsonString);
            }

            MessageBox.Show("Applied to:\n" + sb.ToString());
        }

        private void OnSelectedDeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DevicesList.SelectedItems.Count != 1)
            {
                return;
            }

            string deviceId = ((DeviceSummary)DevicesList.SelectedItem).Name;

            DeviceTwinPanel.Visibility = Visibility.Visible;
            DeviceTwinPanel.LoadAsync(deviceId);
        }

        private void OnCollapse(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        Dictionary<string, DeviceSummary> _devices;
        string _connectionString;
    }
}
