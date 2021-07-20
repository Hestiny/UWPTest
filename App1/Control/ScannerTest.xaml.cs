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
using Windows.UI.Xaml.Shapes;

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

        private Thickness RectMargin =new Thickness(0,0,0,0);

        private double RectWidth
        {
            get { return rectWidth; }
            set { rectWidth = value; }
        }
        private double RectHeight
        {
            get { return rectHeight; }
            set { rectHeight = value; }
        }


        private double wRate;
        private double hRate;
        private double rectWidth;
        private double rectHeight;
        private float zoomValue = 1;
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
            rectWidth = ImageBack.Width = bitmap.PixelWidth;
            rectHeight = ImageBack.Height = bitmap.PixelHeight;
            wRate = myScanner.FlatbedConfiguration.MaxScanArea.Width / ImageBack.Width;
            hRate = myScanner.FlatbedConfiguration.MaxScanArea.Height / ImageBack.Height;
           
            InitRectTransform();

            Scrol.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty , ChangeRectSize);
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

        private async void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await OpenFolder(null, null);
        }

        /// <summary>
        /// 选择导出文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OpenFolder(object sender, TappedRoutedEventArgs e)
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
                await OpenFolder(null, null);
            
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
            try
            {
                var borderPoint = GetRectPoint(Border);
                SelectedScanRegion = new Rect(borderPoint.X * wRate,
                    borderPoint.Y * hRate, 
                    Border.Width * wRate>myScanner.FlatbedConfiguration.MinScanArea.Width? Border.Width * wRate : myScanner.FlatbedConfiguration.MinScanArea.Width, 
                    Border.Height * hRate>myScanner.FlatbedConfiguration.MinScanArea.Height? Border.Height * hRate: myScanner.FlatbedConfiguration.MinScanArea.Height);
               
                ImageScannerResolution imageScannerResolution = new ImageScannerResolution();
                imageScannerResolution.DpiX = 300;
                imageScannerResolution.DpiY = 300;
                myScanner.FlatbedConfiguration.DesiredResolution = imageScannerResolution;//不得超过自动获取的值大小,否则会参数错误
                myScanner.FlatbedConfiguration.SelectedScanRegion = SelectedScanRegion;//xy为起点,w.h为大小,和不能超过最大面积,并且不能小于最小面积
                if (myScanner.FlatbedConfiguration.IsFormatSupported(ImageScannerFormat))//跟设备有关系,本设备不支持输出pdf
                {
                    myScanner.FlatbedConfiguration.Format = ImageScannerFormat;
                }
            }
            catch
            {
                dialog.Hide();
                return;
            }
           
            await myScanner.ScanFilesToFolderAsync(ImageScannerScanSource.Flatbed, storageFolder).AsTask(cancellationToken.Token);//返回扫描进度
            dialog.Hide();
        }

        private void CheckMinScanArea()
        {
            if (SelectedScanRegion.Width < myScanner.FlatbedConfiguration.MinScanArea.Width)
                SelectedScanRegion.Width = myScanner.FlatbedConfiguration.MinScanArea.Width;
            if (SelectedScanRegion.Height < myScanner.FlatbedConfiguration.MinScanArea.Height)
                SelectedScanRegion.Height = myScanner.FlatbedConfiguration.MinScanArea.Height;
        }

        #region 自定义大小框选UI事件方法

        private TranslateTransform LTDragTranslation = new TranslateTransform();
        private TranslateTransform RTDragTranslation = new TranslateTransform();
        private TranslateTransform LBDragTranslation = new TranslateTransform();
        private TranslateTransform RBDragTranslation = new TranslateTransform();
        private TranslateTransform borderTransform = new TranslateTransform();
        private double MinWidth
        {
            get
            {
                return myScanner.FlatbedConfiguration.MinScanArea.Width / myScanner.FlatbedConfiguration.MaxScanArea.Width * ImageBack.Width;
            }
        }
        private double MinHeight
        {
            get
            {
                return myScanner.FlatbedConfiguration.MinScanArea.Height / myScanner.FlatbedConfiguration.MaxScanArea.Height * ImageBack.Height;
            }
        }

        private void InitRectTransform()
        {
            LT.RenderTransform = LTDragTranslation;
            LB.RenderTransform = LBDragTranslation;
            RT.RenderTransform = RTDragTranslation;
            RB.RenderTransform = RBDragTranslation;
            Border.RenderTransform = borderTransform;
        }

        private void ChangeRectSize(DependencyObject sender, DependencyProperty dp)
        {
            RB.Height = RB.Width = LB.Height = LB.Width = RT.Height = RT.Width = LT.Width = LT.Height = 10 / Scrol.ZoomFactor;
        }

        /// <summary>
        /// 更新当前的选框大小
        /// </summary>
        private void UpdateBorder()
        {
            var lt= GetRectPoint(LT);
            var lb = GetRectPoint(LB);
            var rt = GetRectPoint(RT);
            RectMargin.Left = lt.X;
            RectMargin.Top = lt.Y;
            RectWidth = rt.X - lt.X;
            RectHeight = lb.Y - lt.Y;
            //Border.Margin = new Thickness(lt.X, lt.Y, 0, 0);
            Border.Height = rectHeight;
            Border.Width = rectWidth;
        }

        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var rect = sender as Rectangle;
            switch (rect.Tag)
            {
                case "LT":
                    LTRectMove(LTDragTranslation, LBDragTranslation, RTDragTranslation, e);
                    break;
                case "RT":
                    RTRectMove(RTDragTranslation, RBDragTranslation, LTDragTranslation, e);
                    break;
                case "LB":
                    LBRectMove(LBDragTranslation, LTDragTranslation, RBDragTranslation, e);
                    break;
                case "RB":
                    RBRectMove(RBDragTranslation, RTDragTranslation, LBDragTranslation, e);
                    break;
            }
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double x, y;
            x = e.Delta.Translation.X;
            y = e.Delta.Translation.Y;
            if (LTDragTranslation.X + e.Delta.Translation.X < 0)
            {
                x = 0 - LTDragTranslation.X;
            }
            else if (RTDragTranslation.X + e.Delta.Translation.X > 0)
            {
                x = 0 - RTDragTranslation.X;
            }
            if(LBDragTranslation.Y + e.Delta.Translation.Y > 0)
            {
                y = 0 - LBDragTranslation.Y;
            }
           else if(LTDragTranslation.Y + e.Delta.Translation.Y < 0)
            {
                y = 0 - LTDragTranslation.Y;
            }

            borderTransform.X += x;
            borderTransform.Y += y;
            LTDragTranslation.X += x;
            LTDragTranslation.Y += y;
            LBDragTranslation.X += x;
            LBDragTranslation.Y += y;
            RTDragTranslation.X += x;
            RTDragTranslation.Y += y;
            RBDragTranslation.X += x;
            RBDragTranslation.Y += y;
        }

        private void MoveBorder_X(double x)
        {
            borderTransform.X += x;
        }
        private void MoveBorder_Y(double y)
        {
            borderTransform.Y += y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="own">拖动的对象本身</param>
        /// <param name="x">需要在x轴同步的对象</param>
        /// <param name="y">需要在y轴同步的对象</param>
        private void LTRectMove(TranslateTransform own, TranslateTransform x, TranslateTransform y, ManipulationDeltaRoutedEventArgs e)
        {
            if (LTDragTranslation.X + e.Delta.Translation.X / Scrol.ZoomFactor >= 0 &&
                (RTDragTranslation.X + ImageBack.Width - LTDragTranslation.X + e.Delta.Translation.X / Scrol.ZoomFactor >= MinWidth || e.Delta.Translation.X < 0))
            {
                MoveLT_X(own, e);
                MoveLT_X(x, e);
                MoveBorder_X(e.Delta.Translation.X);
            }
            if (LTDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor >= 0 &&
                (LBDragTranslation.Y + ImageBack.Height - LTDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor >= MinHeight || e.Delta.Translation.Y < 0))
            {
                MoveLT_Y(own, e);
                MoveLT_Y(y, e);
                MoveBorder_Y(e.Delta.Translation.Y);
            }
            UpdateBorder();
        }

        private void LBRectMove(TranslateTransform own, TranslateTransform x, TranslateTransform y, ManipulationDeltaRoutedEventArgs e)
        {
            if (LBDragTranslation.X + e.Delta.Translation.X / Scrol.ZoomFactor >= 0 &&
                (RBDragTranslation.X + ImageBack.Width - LBDragTranslation.X + e.Delta.Translation.X / Scrol.ZoomFactor >= MinWidth || e.Delta.Translation.X < 0))
            {
                MoveLT_X(own, e);
                MoveLT_X(x, e);
                MoveBorder_X(e.Delta.Translation.X);
            }
            if (LBDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor <= 0 &&
                 (LBDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor + ImageBack.Height - LTDragTranslation.Y >= MinHeight || e.Delta.Translation.Y > 0))
            {
                MoveLT_Y(own, e);
                MoveLT_Y(y, e);
                MoveBorder_Y(e.Delta.Translation.Y);
            }
            UpdateBorder();
        }

        private void RBRectMove(TranslateTransform own, TranslateTransform x, TranslateTransform y, ManipulationDeltaRoutedEventArgs e)
        {
            if (RBDragTranslation.X + e.Delta.Translation.X / Scrol.ZoomFactor <= 0 &&
                (RBDragTranslation.X + ImageBack.Width + e.Delta.Translation.X / Scrol.ZoomFactor - LBDragTranslation.X >= MinWidth || e.Delta.Translation.X > 0))
            {
                MoveLT_X(own, e);
                MoveLT_X(x, e);
            }
            if (LBDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor <= 0 &&
                (LBDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor + ImageBack.Height - LTDragTranslation.Y >= MinHeight || e.Delta.Translation.Y > 0))
            {
                MoveLT_Y(own, e);
                MoveLT_Y(y, e);
            }
            UpdateBorder();
        }

        private void RTRectMove(TranslateTransform own, TranslateTransform x, TranslateTransform y, ManipulationDeltaRoutedEventArgs e)
        {
            if (RTDragTranslation.X + e.Delta.Translation.X / Scrol.ZoomFactor <= 0 &&
                (RTDragTranslation.X + ImageBack.Width + e.Delta.Translation.X / Scrol.ZoomFactor - LTDragTranslation.X >= MinWidth || e.Delta.Translation.X > 0))
            {
                MoveLT_X(own, e);
                MoveLT_X(x, e);
            }
            if (RTDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor >= 0 &&
               (RBDragTranslation.Y + ImageBack.Height - RTDragTranslation.Y + e.Delta.Translation.Y / Scrol.ZoomFactor >= MinHeight || e.Delta.Translation.Y < 0))
            {
                MoveLT_Y(own, e);
                MoveLT_Y(y, e);
            }
            UpdateBorder();
        }

        /// <summary>
        /// X轴移动
        /// </summary>
        /// <param name="translateTransform"></param>
        /// <param name="e"></param>
        private void MoveLT_X(TranslateTransform translateTransform, ManipulationDeltaRoutedEventArgs e)
        {
            translateTransform.X += e.Delta.Translation.X / Scrol.ZoomFactor;
        }

        /// <summary>
        /// Y轴移动
        /// </summary>
        /// <param name="translateTransform"></param>
        /// <param name="e"></param>
        private void MoveLT_Y(TranslateTransform translateTransform, ManipulationDeltaRoutedEventArgs e)
        {
            translateTransform.Y += e.Delta.Translation.Y / Scrol.ZoomFactor;
        }

        private void TestRect_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var rect = sender as Rectangle;
            rect.ManipulationMode ^= ManipulationModes.TranslateInertia;//ManipulationMode中的所有模式开关都在不同的位数中,可以通过数值直接的位操作进行开关
        }

        /// <summary>
        /// 坐标以左上角的相对位置为准,获取当前控件相对与父控件的位置
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private Point GetRectPoint(Rectangle rect)
        {
            var t = rect.TransformToVisual((UIElement)rect.Parent);
            Point screenCoords = t.TransformPoint(new Point(0, 0));
            return screenCoords;
        }
        #endregion

        private void Priew_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //start = e.GetCurrentPoint((UIElement)sender);
            ////between 0 and MaximumScanArea.Width - 1
            //SelectedScanRegion.X = start.Position.X / (sender as Image).ActualWidth * myScanner.FlatbedConfiguration.MaxScanArea.Width;
            //SelectedScanRegion.Y = start.Position.Y / (sender as Image).ActualHeight * myScanner.FlatbedConfiguration.MaxScanArea.Height;
        }

        private void Priew_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //end = e.GetCurrentPoint((UIElement)sender);
            ////between MinimumScanArea.Width and(MaximumScanArea.Width – SelectedScanRegion.X)
            //SelectedScanRegion.Width = Math.Abs(end.Position.X - start.Position.X) / (sender as Image).ActualWidth * myScanner.FlatbedConfiguration.MaxScanArea.Width;
            //SelectedScanRegion.Height = Math.Abs(end.Position.Y - start.Position.Y) / (sender as Image).ActualHeight * myScanner.FlatbedConfiguration.MaxScanArea.Height;
            //CheckMinScanArea();
        }
    }
}
