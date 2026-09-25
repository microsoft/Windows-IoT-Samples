// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DMMockPortal
{
    class MetricSummary
    {
        public string Name { get; set; }
        public string Query { get; set; }
        public string Count { get; set; }
    }

    public delegate void SaveEventType();
    public delegate void DeleteEventType();

    public partial class DeploymentControl : UserControl
    {
        public SelectedDeploymentChangedEventType SelectedReportedDeploymentChangedEvent;
        public SaveEventType SaveEvent;
        public DeleteEventType DeleteEvent;

        public string DeploymentName
        {
            get
            {
                return DeploymentNameValueBox.Text;
            }
        }

        public void SetConnectionString(string cs)
        {
            _connectionString = cs;
            DeviceListPanel.SetConnectionString(cs);
        }

        public DeploymentControl()
        {
            InitializeComponent();

            DeviceListPanel.SelectedReportedDeploymentChangedEvent += OnSelectedReportedDeploymentChanged;
        }

        private void OnSelectedReportedDeploymentChanged(string deploymentId)
        {
            SelectedDeploymentChangedEventType selectedDeploymentChanged = SelectedReportedDeploymentChangedEvent;
            if (selectedDeploymentChanged != null)
            {
                selectedDeploymentChanged(deploymentId);
            }
        }

        private void OnPropertiesBrowse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                DesiredPropertiesValueBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void OnHelp(object sender, RoutedEventArgs e)
        {
            Dialogs.SamplesDialog sd = new Dialogs.SamplesDialog();
            sd.ShowDialog();
        }

        private void OnTargetRun(object sender, RoutedEventArgs e)
        {
            ConditionRunAsync(TargetConditionValueBox.Text);
        }

        private void SuccessRunAsync(object sender, RoutedEventArgs e)
        {
            if (-1 == MetricsList.SelectedIndex)
            {
                MessageBox.Show("Must select a metric first.");
                return;
            }

            MetricSummary ms = (MetricSummary)MetricsList.SelectedItem;
            string query = ms.Query;
            ConditionRunAsync(query);
        }

        private void OnMetricRunEditAsync(object sender, RoutedEventArgs e)
        {
            if (-1 == MetricsList.SelectedIndex)
            {
                MessageBox.Show("Must select a metric first.");
                return;
            }
            ConditionRunAsync(MetricConditionValueBox.Text);
        }

        private void OnMetricPropertiesPaste(object sender, RoutedEventArgs e)
        {
            MetricConditionNameBox.Text = JsonTemplates.SuccessQueryName;
            MetricConditionValueBox.Text = JsonTemplates.SuccessQueryValue;
        }

        private async void ConditionRunAsync(string condition)
        {
            DevicesQuery allDevicesQuery = new DevicesQuery(condition);
            await allDevicesQuery.Refresh(_connectionString);

            StringBuilder sb = new StringBuilder();
            foreach (var pair in allDevicesQuery.Devices)
            {
                sb.Append(pair.Key);
                sb.Append("\n");
            }
            if (sb.Length != 0)
            {
                MessageBox.Show(sb.ToString());
            }
            else
            {
                MessageBox.Show("No devices found!");
            }
        }

        public async Task CreateDeploymentAsync()
        {
            string configurationId = DeploymentNameValueBox.Text;
            int priority = Int32.Parse(PriorityValueBox.Text);
            string targetCondition = TargetConditionValueBox.Text;

            var registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
            Configuration configuration = new Configuration(configurationId);

            Dictionary<string, object> d = new Dictionary<string, object>();

            JObject dp = (JObject)JsonConvert.DeserializeObject(DesiredPropertiesValueBox.Text);
            foreach (JToken t in dp.Children())
            {
                if (!(t is JProperty))
                {
                    continue;
                }

                JProperty p = (JProperty)t;
                if (p.Value.Type != JTokenType.Object)
                {
                    continue;
                }

                d["properties.desired." + p.Name] = p.Value;
            }

            configuration.Content = new ConfigurationContent();
            configuration.Content.DeviceContent = d;

            for (int i = 0; i < MetricsList.Items.Count; ++i)
            {
                MetricSummary ms = (MetricSummary)MetricsList.Items[i];
                configuration.Metrics.Queries.Add(ms.Name, ms.Query);
            }

            configuration.TargetCondition = targetCondition;
            configuration.Priority = priority;

            await registryManager.AddConfigurationAsync(configuration);
        }

        private void OnImportDeployment(object sender, RoutedEventArgs e)
        {
            string fileContent = "";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Profile files (*.json)|*.json";
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                fileContent = File.ReadAllText(openFileDialog.FileName);

                JObject root = (JObject)JsonConvert.DeserializeObject(fileContent);

                DeploymentNameValueBox.Text = (string)root["name"];
                PriorityValueBox.Text = ((long)root["priority"]).ToString();
                DesiredPropertiesValueBox.Text = ((JObject)root["desiredState"]).ToString();
                TargetConditionValueBox.Text = (string)root["targetCondition"];

                MetricsList.Items.Clear();

                MetricSummary ms0 = new MetricSummary();

                ms0.Name = "successQuery";
                ms0.Query = "SELECT deviceId FROM Devices WHERE " + (string)root["successCondition"];
                ms0.Count = "<unknown>";

                MetricsList.Items.Add(ms0);

                MetricSummary ms1 = new MetricSummary();

                ms1.Name = "failureQuery";
                ms1.Query = "SELECT deviceId FROM Devices WHERE " + (string)root["failureCondition"];
                ms1.Count = "<unknown>";

                MetricsList.Items.Add(ms1);

                MetricSummary ms2 = new MetricSummary();

                ms2.Name = "pendingQuery";
                ms2.Query = "SELECT deviceId FROM Devices WHERE " + (string)root["pendingCondition"];
                ms2.Count = "<unknown>";

                MetricsList.Items.Add(ms2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private async void DeleteDeploymentAsync()
        {
            string deploymentId = DeploymentNameValueBox.Text;

            if (deploymentId == DMConstants.AllDevicesConfigName)
            {
                MessageBox.Show("Cannot delete the *All* configuration.");
                return;
            }

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
            await registryManager.RemoveConfigurationAsync(deploymentId);

            DeleteEventType deleteEvent = DeleteEvent;
            if (deleteEvent != null)
            {
                deleteEvent();
            }

            MessageBox.Show("'" + DeploymentName + "' has been deleted successfully.");
        }

        private void OnDeleteDeployment(object sender, RoutedEventArgs e)
        {
            DeleteDeploymentAsync();
        }

        public void Show(DeploymentSummary deploymentSummary)
        {
            TargetConditionValueBox.Text = deploymentSummary.AzureConfiguration.TargetCondition;

            MetricsList.Items.Clear();

            foreach (var p in deploymentSummary.AzureConfiguration.Metrics.Queries)
            {
                MetricSummary ms = new MetricSummary();
                ms.Name = p.Key;
                ms.Query = p.Value;

                if (deploymentSummary.AzureConfiguration.Metrics.Results.Keys.Contains(p.Key))
                {
                    ms.Count = deploymentSummary.AzureConfiguration.Metrics.Results[p.Key].ToString();
                }
                else
                {
                    ms.Count = "<unknown>";
                }

                MetricsList.Items.Add(ms);
            }

            SystemMetricsList.Items.Clear();

            if (deploymentSummary.AzureConfiguration.SystemMetrics != null)
            {
                foreach (var p in deploymentSummary.AzureConfiguration.SystemMetrics.Queries)
                {
                    MetricSummary ms = new MetricSummary();
                    ms.Name = p.Key;
                    ms.Query = p.Value;

                    string resultCount = "<unknown>";
                    if (deploymentSummary.AzureConfiguration.SystemMetrics.Results.Keys.Contains(p.Key))
                    {
                        long count = deploymentSummary.AzureConfiguration.SystemMetrics.Results[p.Key];
                        resultCount = count.ToString();
                    }
                    ms.Count = resultCount;

                    SystemMetricsList.Items.Add(ms);
                }
            }

            MetricConditionValueBox.Text = "";

            PriorityValueBox.Text = deploymentSummary.AzureConfiguration.Priority.ToString();

            DeploymentNameValueBox.Text = deploymentSummary.AzureConfiguration.Id;

            StringBuilder sb1 = new StringBuilder();

            if (deploymentSummary.AzureConfiguration.Content?.DeviceContent != null)
            {
                IDictionary<string, object> d = deploymentSummary.AzureConfiguration.Content.DeviceContent;

                JObject desiredPropertiesObject = new JObject();
                foreach (var p in d)
                {
                    string path = p.Key;
                    string[] parts = path.Split('.');
                    if (parts.Length == 3 && parts[0] == "properties" && parts[1] == "desired")
                    {
                        desiredPropertiesObject[parts[2]] = (JToken)p.Value;
                    }
                }

                DesiredPropertiesValueBox.Text = desiredPropertiesObject.ToString();
            }

            /* ToDo: When module support is added, we need to figure out what to doe with ModuleContent.

            if (deploymentSummary.AzureConfiguration.Content?.ModulesContent != null)
            {
                foreach (var p in deploymentSummary.AzureConfiguration.Content.ModulesContent)
                {
                    if (sb1.Length > 0)
                        sb1.Append("\n");

                    sb1.Append("[M]\n");
                    sb1.Append("  [K] " + p.Key + "\n");
                    if (p.Value is IDictionary<string, object>)
                    {
                        foreach (var pi in p.Value)
                        {
                            if (sb1.Length > 0)
                                sb1.Append("\n");

                            sb1.Append("  [V]\n");
                            sb1.Append("  " + pi.Key);
                            sb1.Append(" : ");
                            sb1.Append(pi.Value);
                        }
                    }
                    else
                    {
                        sb1.Append("  [V] " + p.Value + "\n");
                    }
                }
            }
            */

            _deploymentSummary = deploymentSummary;
        }

        private void OnAddMetricCondition(object sender, RoutedEventArgs e)
        {
            MetricSummary ms = new MetricSummary();
            ms.Name = MetricConditionNameBox.Text;
            ms.Query = MetricConditionValueBox.Text;

            MetricsList.Items.Add(ms);
        }

        private void OnRemoveMetricCondition(object sender, RoutedEventArgs e)
        {
            if (MetricsList.SelectedIndex == -1)
            {
                MessageBox.Show("Select a metric to remove.");
                return;
            }

            MetricsList.Items.RemoveAt(MetricsList.SelectedIndex);
        }

        private void OnMetricChanged(object sender, RoutedEventArgs e)
        {
            if (-1 == MetricsList.SelectedIndex)
            {
                return;
            }

            MetricSummary ms = (MetricSummary)MetricsList.SelectedItem;
            string query = ms.Query;

            MetricConditionNameBox.Text = ms.Name;
            MetricConditionValueBox.Text = ms.Query;
        }

        private void OnShowDeviceListPanel(object sender, RoutedEventArgs e)
        {
            if (DeviceListPanel.Visibility == Visibility.Collapsed)
            {
                DeviceListPanel.Visibility = Visibility.Visible;
            }

            DeviceListPanel.LoadDeploymentDevicesAsync(TargetConditionValueBox.Text);
        }

        private void OnSaveDeployment(object sender, RoutedEventArgs e)
        {
            SaveEventType saveEvent = SaveEvent;
            if (saveEvent != null)
            {
                saveEvent();
            }
        }

        private void OnExportDeployment(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ToDo: Export deployment definition to a json file.");
        }

        private void OnCollapse(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnTabClicked(object sender, RoutedEventArgs e)
        {
            RadioButton cb = (RadioButton)sender;
            Grid c = (Grid)cb.Parent;

            uint targetIndex = 0;
            foreach (var element in c.Children)
            {
                if (element is RadioButton)
                {
                    RadioButton ccb = (RadioButton)element;
                    if (ccb == cb)
                    {
                        break;
                    }
                    ++targetIndex;
                }
            }

            uint index = 0;
            foreach (UIElement element in TabContentGrid.Children)
            {
                if (index == targetIndex)
                {
                    element.Visibility = Visibility.Visible;
                }
                else
                {
                    element.Visibility = Visibility.Collapsed;
                }
                ++index;
            }
        }

        string _connectionString;
        DeploymentSummary _deploymentSummary;
    }
}
