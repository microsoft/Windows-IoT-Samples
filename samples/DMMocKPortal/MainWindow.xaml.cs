// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace DMMockPortal
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            _initialized = false;

            InitializeComponent();

            LoadConnectionString();
            DeploymentListPanel.SetConnectionString(IoTHubConnectionStringBox.Text);

            _initialized = true;

            DeploymentListPanel.RefreshDeploymentListAsync();
        }

        private void LoadConnectionString()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(DMConstants.RegistryStore);
            if (key == null)
            {
                return;
            }
            IoTHubConnectionStringBox.Text = (string)key.GetValue(DMConstants.RegistryConnectionString);
        }

        private void SaveConnectionString()
        {
            if (!_initialized)
            {
                return;
            }

            RegistryKey key = Registry.CurrentUser.CreateSubKey(DMConstants.RegistryStore);
            if (key == null)
            {
                return;
            }

            key.SetValue(DMConstants.RegistryConnectionString, IoTHubConnectionStringBox.Text);
        }

        private void OnConnectionStringChanged(object sender, TextChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            SaveConnectionString();
            DeploymentListPanel.SetConnectionString(IoTHubConnectionStringBox.Text);
        }

        bool _initialized;
    }
}
