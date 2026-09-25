// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DMMockPortal
{
    public delegate void ApplyPropertiesEventType(string propertiesJsonString);
    public delegate void DeploymentSelectedEventType(string deploymentId);

    public partial class DeviceTwinControl : UserControl
    {
        public ApplyPropertiesEventType ApplyPropertiesEvent;
        public DeploymentSelectedEventType SelectedDeploymentChangedEvent;

        public DeviceTwinControl()
        {
            InitializeComponent();
        }

        public void SetConnectionString(string cs)
        {
            _connectionString = cs;
        }

        public async void LoadAsync(string deviceId)
        {
            ReportedDeployments.Items.Clear();

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(_connectionString);

            var deviceTwin = await registryManager.GetTwinAsync(deviceId);
            if (deviceTwin == null)
            {
                MessageBox.Show("An error occured while retrieving the device twin data.");
                return;
            }

            JObject tags = (JObject)JsonConvert.DeserializeObject(deviceTwin.Tags.ToJson());

            JObject desiredValue = (JObject)JsonConvert.DeserializeObject(deviceTwin.Properties.Desired.ToJson());
            JObject desiredFilteredValue = new JObject();

            foreach (JProperty p in desiredValue.Children())
            {
                if (p.Name == "$metadata")
                {
                    continue;
                }
                desiredFilteredValue[p.Name] = p.Value;
            }

            JObject reportedValue = (JObject)JsonConvert.DeserializeObject(deviceTwin.Properties.Reported.ToJson());
            JObject reportedFilteredValue = new JObject();

            foreach (JProperty p in reportedValue.Children())
            {
                if (p.Name == "$metadata")
                {
                    continue;
                }
                reportedFilteredValue[p.Name] = p.Value;

                if (p.Value.Type==JTokenType.Object)
                {
                    JObject valueObject = (JObject)p.Value;
                    JToken metaValueToken = valueObject[JsonTemplates.Meta];
                    if (metaValueToken != null && metaValueToken.Type==JTokenType.Object)
                    {
                        JObject metaObject = (JObject)metaValueToken;
                        JToken deploymentIdToken = metaObject[JsonTemplates.DeploymentId];
                        if (deploymentIdToken != null && deploymentIdToken.Type==JTokenType.String)
                        {
                            string deploymentId = (string)deploymentIdToken;
                            if (deploymentId != JsonTemplates.DeploymentIdUnspecified)
                            {
                                ReportedDeployments.Items.Add(deploymentId);
                            }
                        }
                    }
                }
            }

            DeviceName.Text = deviceId;
            TagsValueBox.Text = tags.ToString();
            DesiredPropretiesValueBox.Text = desiredFilteredValue.ToString();
            ReportedPropertiesValueBox.Text = reportedFilteredValue.ToString();
        }

        private void OnApplyDesiredProperties(object sender, RoutedEventArgs e)
        {
            var applyPropertiesEvent = ApplyPropertiesEvent;
            if (applyPropertiesEvent != null)
            {
                applyPropertiesEvent(DesiredPropretiesValueBox.Text);
            }
        }

        private void OnApplyTags(object sender, RoutedEventArgs e)
        {
            var applyPropertiesEvent = ApplyPropertiesEvent;
            if (applyPropertiesEvent != null)
            {
                applyPropertiesEvent(TagsValueBox.Text);
            }
        }

        private void OnTagsPaste(object sender, RoutedEventArgs e)
        {
            TagsValueBox.Text = JsonTemplates.Tags;
        }

        private void OnPropertiesPaste(object sender, RoutedEventArgs e)
        {
            DesiredPropretiesValueBox.Text = JsonTemplates.DesiredProperties;
        }

        private void OnTagsBrowse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TagsValueBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void OnPropertiesBrowse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                DesiredPropretiesValueBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void ReportedDeploymentsSelectionChanged()
        {
            if (ReportedDeployments.SelectedItems.Count != 1)
            {
                return;
            }

            string selectedId = (string)ReportedDeployments.SelectedItem;

            var deploymentSelectedEvent = SelectedDeploymentChangedEvent;
            if (deploymentSelectedEvent != null)
            {
                deploymentSelectedEvent(selectedId);
            }
        }

        private void OnReportedDeploymentsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReportedDeploymentsSelectionChanged();
        }

        private void OnCollapse(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnTabClicked(RadioButton cb, Grid targetGrid)
        {
            if (cb.IsChecked != true)
            {
                return;
            }

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
            foreach (UIElement element in targetGrid.Children)
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


        private void OnDesiredTabClicked(object sender, RoutedEventArgs e)
        {
            OnTabClicked((RadioButton)sender, DesiredTabContentGrid);
        }

        private void OnReportedTabClicked(object sender, RoutedEventArgs e)
        {
            OnTabClicked((RadioButton)sender, ReportedTabContentGrid);
        }

        string _connectionString;
    }
}
