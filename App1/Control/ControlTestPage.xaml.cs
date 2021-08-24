using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App1.Control
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ControlTestPage : Page
    {
        public ControlTestPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            ControlGrid.Children.Clear();
            TestControl testControl = new TestControl();
            testControl.ControlTag = "11111";
            ControlGrid.Children.Add(testControl);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            ControlGrid.Children.Clear();
            TestControl testControl = new TestControl();
            testControl.ControlTag = "2222";
            ControlGrid.Children.Add(testControl);
        }

    }
}
