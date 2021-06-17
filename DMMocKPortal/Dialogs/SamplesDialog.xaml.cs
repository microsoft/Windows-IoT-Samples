// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Windows;

namespace DMMockPortal.Dialogs
{
    public partial class SamplesDialog : Window
    {
        public SamplesDialog()
        {
            InitializeComponent();
            SamplePropertyConfiguration.Text = JsonTemplates.DesiredProperties;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
