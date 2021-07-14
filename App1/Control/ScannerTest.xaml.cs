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
using Windows.Storage;
using Windows.UI.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App1.Control
{
    public sealed partial class ScannerTest : UserControl
    {
        public ScannerTest()
        {
            this.InitializeComponent();
            InitScanner();
        }

        private int ImageColorMode
        {
            get{ return (int)ImageScannerColorMode; }
            set{ ImageScannerColorMode = (ImageScannerColorMode)value; }
        }

        private int ImageScanSoure
        {
            get { return (int)imageScannerScanSource; }
            set { imageScannerScanSource = (ImageScannerScanSource)value; }
        }

        private int BrightValue
        {
            get { return brightValue; }
            set { brightValue = value; }
        }

        private int brightValue=50;
        private DeviceWatcher scannerWatcher;
        private ImageScanner myScanner;
        private ImageScannerFlatbedConfiguration imageScannerFlatbedConfiguration;
        private ImageScannerColorMode ImageScannerColorMode;
        private ImageScannerFormat ImageScannerFormat = ImageScannerFormat.Pdf;
        private Rect SelectedScanRegion;
        private ImageScannerScanSource imageScannerScanSource = ImageScannerScanSource.AutoConfigured;
        StorageFolder storageFolder;
        /// <summary>
        /// 预览弹窗
        /// </summary>
        ContentDialog dialog;

        public void InitDeviceWatcher()
        {
            // Create a Device Watcher class for type Image Scanner for enumerating scanners
            scannerWatcher = DeviceInformation.CreateWatcher(DeviceClass.ImageScanner);

        }

        private async Task InitScanner()
        {
            try
            {
                DeviceInformationCollection device = await DeviceInformation.FindAllAsync(DeviceClass.ImageScanner);
                if (device.Count == 0) 
                    return;

                myScanner = await ImageScanner.FromIdAsync(device.FirstOrDefault().Id);
                imageScannerFlatbedConfiguration = myScanner.FlatbedConfiguration;
                ImageColorMode = (int)imageScannerFlatbedConfiguration.ColorMode;
                ImageScanSoure = (int)myScanner.DefaultScanSource;
                ScannerName.ItemsSource = device;
                ScannerName.SelectedItem = device.FirstOrDefault();
            }
            catch
            {
                return;
            }

            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await InitScanner();        
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
            ImageBack.Visibility = Visibility.Visible;
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            Priew.Source = bitmap;
            //await Task.Delay(20);
            ImageBack.Width = bitmap.PixelWidth;
            ImageBack.Height = bitmap.PixelHeight;
        }

        /// <summary>
        /// 刷新扫描仪列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cancellationToken = new CancellationTokenSource();
            ShowPriewDialog(cancellationToken);

            myScanner.FlatbedConfiguration.ColorMode = ImageScannerColorMode;
            //myScanner.FlatbedConfiguration = imageScannerFlatbedConfiguration;
            IRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (myScanner.FlatbedConfiguration.BrightnessStep != 0)//步长为0则不能设置亮度
                myScanner.FlatbedConfiguration.Brightness = BrightValue / 100 * (myScanner.FlatbedConfiguration.MaxBrightness - myScanner.FlatbedConfiguration.MinBrightness);
            else
                BrightValue = 50;

            var result = await myScanner.ScanPreviewToStreamAsync(
                      ImageScannerScanSource.Flatbed, stream).AsTask(cancellationToken.Token);
             await ShowImage(stream);

            dialog.Hide();
        }

        /// <summary>
        /// 切换扫描仪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ScannerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem==null) 
                return;

            DeviceInformation device = (DeviceInformation)(sender as ComboBox).SelectedItem;
            myScanner = await ImageScanner.FromIdAsync(device.Id);
        }

        /// <summary>
        /// 选择导出文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                storageFolder = folder;
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                FilePath.Text = "Picked folder: " + folder.Name;
            }
            else
            {
                FilePath.Text = "Operation cancelled.";
            }
        }

        /// <summary>
        /// 扫描  //TODO:在线程还没挂载到扫描之前就点击弹窗取消
        /// 所有的参数设置都需要检查是否符合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (storageFolder == null)
                TextBlock_Tapped(null, null);
            
            var cancellationToken = new CancellationTokenSource();
          
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
            
        
            ImageScannerResolution imageScannerResolution = new ImageScannerResolution();
            imageScannerResolution.DpiX = 100;
            imageScannerResolution.DpiY = 100;
            myScanner.FlatbedConfiguration.DesiredResolution = imageScannerResolution;//不得超过自动获取的值大小,否则会参数错误
            myScanner.FlatbedConfiguration.SelectedScanRegion = SelectedScanRegion;//xy为起点,w.h为大小,和不能超过最大面积,并且不能小于最小面积
            if (myScanner.FlatbedConfiguration.IsFormatSupported(ImageScannerFormat))//跟设备有关系,本设备不支持输出pdf
            {
                myScanner.FlatbedConfiguration.Format = ImageScannerFormat;
            }
            await myScanner.ScanFilesToFolderAsync(ImageScannerScanSource.Flatbed, storageFolder).AsTask(cancellationToken.Token);//返回扫描进度
            dialog.Hide();
        }

        private PointerPoint start, end;

        private void CheckMinScanArea()
        {
            if (SelectedScanRegion.Width < myScanner.FlatbedConfiguration.MinScanArea.Width)
                SelectedScanRegion.Width = myScanner.FlatbedConfiguration.MinScanArea.Width;
            if (SelectedScanRegion.Height < myScanner.FlatbedConfiguration.MinScanArea.Height)
                SelectedScanRegion.Height = myScanner.FlatbedConfiguration.MinScanArea.Height;
        }

        private void Priew_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            start = e.GetCurrentPoint((UIElement)sender);
            var t = LT.TransformToVisual((UIElement)LT.Parent);
            Point screenCoords = t.TransformPoint(new Point(0, 0));
            //between 0 and MaximumScanArea.Width - 1
            SelectedScanRegion.X= start.Position.X / (sender as Image).ActualWidth * myScanner.FlatbedConfiguration.MaxScanArea.Width;
            SelectedScanRegion.Y = start.Position.Y / (sender as Image).ActualHeight * myScanner.FlatbedConfiguration.MaxScanArea.Height;
        }

        private Point Point = new Point();
        private TranslateTransform dragTranslation;
        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (dragTranslation == null)
            {
                dragTranslation = new TranslateTransform();
            }
            LT.RenderTransform = dragTranslation;
            dragTranslation.X += e.Delta.Translation.X;
            dragTranslation.Y += e.Delta.Translation.Y;
        }

        private void TestRect_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LT.ManipulationMode ^= ManipulationModes.TranslateInertia;//ManipulationMode中的所有模式开关都在不同的位数中,可以通过数值直接的位操作进行开关
            Point = e.Position;
        }


        private void Priew_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            end = e.GetCurrentPoint((UIElement)sender);
            //between MinimumScanArea.Width and(MaximumScanArea.Width – SelectedScanRegion.X)
            SelectedScanRegion.Width = Math.Abs(end.Position.X - start.Position.X) / (sender as Image).ActualWidth * myScanner.FlatbedConfiguration.MaxScanArea.Width;
            SelectedScanRegion.Height = Math.Abs(end.Position.Y - start.Position.Y) / (sender as Image).ActualHeight * myScanner.FlatbedConfiguration.MaxScanArea.Height;
            CheckMinScanArea();
        }
    }
}
