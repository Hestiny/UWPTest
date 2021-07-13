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
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using Windows.Storage.Streams;
using System.Threading;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App1.Control
{
    public sealed partial class ScannerTest : UserControl
    {
        public ScannerTest()
        {
            this.InitializeComponent();
        }

        private DeviceWatcher scannerWatcher;
        private ImageScanner myScanner;
        /// <summary>
        /// 预览弹窗
        /// </summary>
        ContentDialog dialog;

        public void InitDeviceWatcher()
        {
            // Create a Device Watcher class for type Image Scanner for enumerating scanners
            scannerWatcher = DeviceInformation.CreateWatcher(DeviceClass.ImageScanner);

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceInformationCollection device = await DeviceInformation.FindAllAsync(DeviceClass.ImageScanner);
               myScanner = await ImageScanner.FromIdAsync(device.FirstOrDefault().Id);
                ScannerName.ItemsSource = device;
            }
            catch
            {
                return;
            }
        }

        private async void ScannerName_ItemClick(object sender, ItemClickEventArgs e)
        {
            DeviceInformation device = (DeviceInformation)e.ClickedItem;
            myScanner = await ImageScanner.FromIdAsync(device.Id);
        }

        /// <summary>
        /// 预览扫描对话框
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void ShowPriewDialog(CancellationTokenSource cancellationToken)
        {
            dialog = new ContentDialog();
            dialog.Title = new TextBlock { Text = "扫描中...", FontSize = 14 };
            dialog.PrimaryButtonText = "取消";
            dialog.PrimaryButtonClick += (s, a) =>
            {
                cancellationToken.Cancel();
                cancellationToken.Token.Register(() =>
                {
                    dialog.Hide();
                });
            };
            dialog.ShowAsync();
        }

        /// <summary>
        /// 将预览图加入到控件
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task ShowImage(IRandomAccessStream stream)
        {
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            Priew.Source = bitmap;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cancellationToken = new CancellationTokenSource();
            ShowPriewDialog(cancellationToken);

            IRandomAccessStream stream = new InMemoryRandomAccessStream();
            var result = await myScanner.ScanPreviewToStreamAsync(
                      ImageScannerScanSource.Flatbed, stream).AsTask(cancellationToken.Token);

            await ShowImage(stream);

            dialog.Hide();
        }
    }
}
