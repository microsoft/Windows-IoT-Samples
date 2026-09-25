// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DMMockPortal
{
    public class DeploymentSummary
    {
        public string Name { get; set; }
        public string FailedCount { get; set; }
        public string SuccessCount { get; set; }
        public string AppliedCount { get; set; }
        public string TargetedCount { get; set; }
        public string PendingCount { get; set; }
        public Configuration AzureConfiguration { get; set; }
    }

    public partial class DeploymentListControl : UserControl
    {
        public DeploymentListControl()
        {
            InitializeComponent();

            DeploymentPanel.SelectedReportedDeploymentChangedEvent += OnSelectedReportedDeploymentChanged;
            DeploymentPanel.SaveEvent += CreateDeploymentAsync;
            DeploymentPanel.DeleteEvent += OnDeploymentDeletedEvent;
        }

        public void SetConnectionString(string cs)
        {
            _connectionString = cs;
            DeploymentPanel.SetConnectionString(cs);
        }

        private DeploymentSummary SummarizeConfiguration(Configuration configuration)
        {
            DeploymentSummary cs = new DeploymentSummary();

            cs.AzureConfiguration = configuration;

            cs.Name = configuration.Id;
            cs.FailedCount = "-";
            if (configuration.Metrics.Results.ContainsKey("failureQuery"))  // Sometimes, the query hasn't been run yet.
            {
                cs.FailedCount = configuration.Metrics.Results["failureQuery"].ToString();
            }

            cs.SuccessCount = "-";
            if (configuration.Metrics.Results.ContainsKey("successQuery"))
            {
                cs.SuccessCount = configuration.Metrics.Results["successQuery"].ToString();
            }

            cs.PendingCount = "-";
            if (configuration.Metrics.Results.ContainsKey("pendingQuery"))
            {
                cs.PendingCount = configuration.Metrics.Results["pendingQuery"].ToString();
            }

            cs.AppliedCount = "-";
            if (configuration.SystemMetrics.Results.ContainsKey("appliedCount"))
            {
                cs.AppliedCount = configuration.SystemMetrics.Results["appliedCount"].ToString();
            }

            cs.TargetedCount = "-";
            if (configuration.SystemMetrics.Results.ContainsKey("targetedCount"))
            {
                cs.TargetedCount = configuration.SystemMetrics.Results["targetedCount"].ToString();
            }

            return cs;
        }

        private void RebuildDeploymentList()
        {
            DeploymentsList.Items.Clear();

            if (_deploymentSummaries == null)
            {
                return;
            }

            foreach (DeploymentSummary ds in _deploymentSummaries)
            {
                if (FilterHasErrorsCheckBox.IsChecked == true && ds.FailedCount == "-")
                {
                    continue;
                }

                if (FilterHasPendingCheckBox.IsChecked == true && ds.PendingCount == "-")
                {
                    continue;
                }
                DeploymentsList.Items.Add(ds);
            }
        }

        private async Task LoadDeploymentsAsync()
        {
            if (String.IsNullOrEmpty(_connectionString) || _connectionString == "<connection string>")
            {
                return;
            }

            int maxDeploymentCount = 20;
            RegistryManager registryManager;

            try
            {
                registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to connect to IoTHub.\n\nMake sure you are using the iothubowner connection string (not a device connection string).\n\nIoT Hub Error: " + e.Message, "Connection Error", MessageBoxButton.OK);
                return;
            }

            try
            {
                _deploymentSummaries = new List<DeploymentSummary>();

                DevicesQuery allDevicesQuery = new DevicesQuery("");
                await allDevicesQuery.Refresh(_connectionString);

                DeploymentSummary cs = new DeploymentSummary();
                cs.Name = DMConstants.AllDevicesConfigName;
                cs.AzureConfiguration = new Configuration(DMConstants.AllDevicesConfigName);
                cs.AzureConfiguration.Priority = 1;
                cs.AppliedCount = "-";
                cs.FailedCount = allDevicesQuery.FailedDeviceCount.ToString();
                cs.SuccessCount = allDevicesQuery.SuccessDeviceCount.ToString();
                cs.TargetedCount = allDevicesQuery.DeviceCount.ToString();
                cs.PendingCount = allDevicesQuery.PendingDeviceCount.ToString();

                _deploymentSummaries.Add(cs);

                IEnumerable<Configuration> configurations = await registryManager.GetConfigurationsAsync(maxDeploymentCount);
                foreach (var configuration in configurations)
                {
                    _deploymentSummaries.Add(SummarizeConfiguration(configuration));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to enumerate existing deployments.", "Enumeration Error", MessageBoxButton.OK);
                return;
            }
        }

        // ToDo: This probably belongs in the DeploymentControl.
        private async void CreateDeploymentAsync()
        {
            foreach (var ds in _deploymentSummaries)
            {
                if (ds.AzureConfiguration.Id == DeploymentPanel.DeploymentName)
                {
                    if (MessageBox.Show("Replace existing deployment?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        return;
                    }

                    RegistryManager registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
                    await registryManager.RemoveConfigurationAsync(ds.AzureConfiguration.Id);
                    break;
                }
            }

            try
            {
                await DeploymentPanel.CreateDeploymentAsync();

                MessageBox.Show("'" + DeploymentPanel.DeploymentName + "' has been saved successfully.");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }

            RefreshDeploymentListAsync();
        }

        private void OnDeploymentDeletedEvent()
        {
            RefreshDeploymentListAsync();
        }

        public async void RefreshDeploymentListAsync()
        {
            if (String.IsNullOrEmpty(_connectionString))
            {
                return;
            }

            await LoadDeploymentsAsync();
            RebuildDeploymentList();
        }

        private void OnSelectedReportedDeploymentChanged(string deploymentId)
        {
            if (DeploymentsList.SelectedIndex != -1)
            {
                if (((DeploymentSummary)DeploymentsList.SelectedItem).Name == deploymentId)
                {
                    return;
                }
            }

            int index = -1;
            DeploymentSummary selectedDeployment = null;
            foreach (DeploymentSummary ds in DeploymentsList.Items)
            {
                ++index;

                if (ds.Name == deploymentId)
                {
                    selectedDeployment = ds;
                    break;
                }
            }

            if (index == -1)
            {
                MessageBox.Show("Deployment is not found.");
                return;
            }

            DeploymentsList.SelectedIndex = index;

            DeploymentPanel.Show(selectedDeployment);
        }

        private void OnAddDeployment(object sender, RoutedEventArgs e)
        {
            DeploymentPanel.Visibility = Visibility.Visible;
        }

        private void OnRefreshDeploymentList(object sender, RoutedEventArgs e)
        {
            RefreshDeploymentListAsync();
        }

        private void OnFilterHasErrors(object sender, RoutedEventArgs e)
        {
            RefreshDeploymentListAsync();
        }

        private void OnFilterHasPending(object sender, RoutedEventArgs e)
        {
            RefreshDeploymentListAsync();
        }

        private void OnSelectedDeploymentChanged(object sender, RoutedEventArgs e)
        {
            if (DeploymentsList.SelectedIndex == -1)
            {
                return;
            }

            DeploymentPanel.Visibility = Visibility.Visible;
            DeploymentPanel.Show((DeploymentSummary)DeploymentsList.SelectedItem);
        }

        private DeploymentSummary GetSelectedDeployment()
        {
            if (DeploymentsList.SelectedIndex != 1)
            {
                return null;
            }

            return (DeploymentSummary)DeploymentsList.SelectedItem;
        }

        List<DeploymentSummary> _deploymentSummaries;
        string _connectionString;
    }
}
