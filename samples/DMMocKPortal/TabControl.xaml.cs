// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DMMockPortal
{
    public class TabPage
    {
        public TabPage() { }
        public string Title { get; set; }
        public UIElement Content { get; set; }
    }

    public class TabPages
    {
        public TabPages() { }
        public List<TabPage> Pages { get; set; }
    }

    public class Title
    {
        public Title() { }
        public string TheTitle { get; set; }
    }

    public partial class TabControl : UserControl
    {
        public static readonly DependencyProperty TabPagesProperty = DependencyProperty.Register(
           "TabPages", typeof(List<TabPage>), typeof(TabControl), new PropertyMetadata(false));

        public TabPages TabPages
        {
            get { return (TabPages)this.GetValue(TabPagesProperty); }
            set { this.SetValue(TabPagesProperty, value); }
        }

        public TabControl()
        {
            InitializeComponent();
        }

        private void OnTabClicked(object sender, RoutedEventArgs e)
        {
            RadioButton cb = (RadioButton)sender;
            Canvas c = (Canvas)cb.Parent;

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
            foreach (UIElement element in ContentGrid.Children)
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

            /*
            uint index = 0;
            if (ContentPresenterPart.Content != null)
            {
                foreach (UIElement element in ((Grid)ContentPresenterPart.Content).Children)
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
            */
        }
    }
}
