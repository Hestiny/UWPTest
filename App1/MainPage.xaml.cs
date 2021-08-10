using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Aspose.Words;
using Windows.Storage;
using Spire;
using Spire.Doc;
using Spire.Pdf;
using OfficeConverter;
using LibreOfficeLibrary;
using App1.Control;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    public sealed class Employee : INotifyPropertyChanged
    {
        private string color;
        private double opacity;

        public string Color
        {
            get { return color; }
            set
            {
                color = value;
               RaisePropertyChanged("Color");
            }
        }

        public double Opacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
               RaisePropertyChanged("Opacity");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public UnderlineType Underline = UnderlineType.Wave;
        Employee employee=new Employee();
        string Color = "#FFFF1111";
        public MainPage()
        {
            employee.Color = "#FFFF1111";
            employee.Opacity = 1;
            this.InitializeComponent();
            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            //Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            TitleBarButton.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextRange textRange1 = new TextRange()
            {
                StartIndex = 0,
                Length = rt.Text.Length
            };
            TextHighlighter highlighter = new TextHighlighter()
            {
                Background = richEditBox.Foreground,
                Ranges = { textRange1 }
            };

            rt.TextHighlighters.Clear();
            rt.TextHighlighters.Add(highlighter);

            richEditBox.Document.SetText(TextSetOptions.None, new string((char)160,10));
            Windows.UI.Text.ITextSelection selectedText = richEditBox.Document.Selection;
            ITextRange textRange = richEditBox.Document.GetRange(0, 4);
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                if (charFormatting.Underline == Windows.UI.Text.UnderlineType.None)
                {
                    textRange.CharacterFormat.Underline = Windows.UI.Text.UnderlineType.Wave;
                    charFormatting.Underline = Windows.UI.Text.UnderlineType.Wave;
                }
                else
                {
                    textRange.CharacterFormat.Underline = Windows.UI.Text.UnderlineType.None;
                    charFormatting.Underline = Windows.UI.Text.UnderlineType.Wave;
                }
                //selectedText.CharacterFormat = charFormatting;
            }
        }

        private void richEditBox_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private  void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            employee.Color= "#" + ((int)((e.NewValue / 100) * 255)).ToString("X2") + employee.Color.Substring(3);
            var m = box.Foreground;
            box.Foreground.Opacity= e.NewValue / 100;
            box.IsHitTestVisible = true; 
            VisualStateManager.GoToState(box, "PointerOver", true);

            wite();
        }
        private async void wite()
        {
            await Task.Delay(10);
            VisualStateManager.GoToState(box, "Normal", true);
            box.IsHitTestVisible = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //必须刷新状态才能更新前景色
            box.IsHitTestVisible = true;
            VisualStateManager.GoToState(box, "Normal", true); 
            box.IsHitTestVisible = false;
        }

        private void CloseAllPage()
        {
            ControlTestPage.Visibility = Visibility.Collapsed;
            wordTopdf.Visibility = Visibility.Collapsed;
            ThemeTest.Visibility = Visibility.Collapsed;
            ControlTest.Visibility = Visibility.Collapsed;
            ScannerTest.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CloseAllPage();
            ControlTestPage.Visibility = Visibility.Visible;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CloseAllPage();
            wordTopdf.Visibility = Visibility.Visible;
        }

        #region 测试word转pdf
        //需要付费,包含水印
        public async static void ConverDocToPdf(StorageFile docfile)
        {
            //打开word文档,将doc文档转为pdf文档
          
            var resourcepath = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Resources", CreationCollisionOption.OpenIfExists);
            StorageFile wordFile = await docfile.CopyAsync(resourcepath, "Newpdf", NameCollisionOption.ReplaceExisting);
            Aspose.Words.Document doc = new Aspose.Words.Document(wordFile.Path);

            if (doc != null)
            {
                var tempImageFile = await resourcepath.CreateFileAsync("Newpdf.pdf", CreationCollisionOption.GenerateUniqueName);
                doc.Save(tempImageFile.Path, SaveFormat.Pdf);
                //StorageFile pdffile = await StorageFile.GetFileFromPathAsync(resourcepath + "\\Newpdf");
                var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");
                Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
                //StorageFolder outputfolder =await StorageFolder.GetFolderFromPathAsync("C:\\Users\\kdanmobile\\Desktop\\pdf");
                //StorageFolder outputfolder = await tempImageFile.GetParentAsync();
                await tempImageFile.CopyAsync(outputfolder,"Newpdf.pdf",NameCollisionOption.ReplaceExisting);
            }
        }

        //接口无法调用
        public async static void WordToPDF(StorageFile docfile)
        {
            var resourcepath = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Resources", CreationCollisionOption.OpenIfExists);
            StorageFile wordFile = await docfile.CopyAsync(resourcepath, "Newpdf", NameCollisionOption.ReplaceExisting);
            var tempImageFile = await resourcepath.CreateFileAsync("Newpdf.pdf", CreationCollisionOption.ReplaceExisting);

            Microsoft.Office.Interop.Word.Application application = new Microsoft.Office.Interop.Word.Application();
            Microsoft.Office.Interop.Word.Document document = null;
            try
            {
               application.Visible = false;
                document = application.Documents.Open(wordFile.Path);
                document.ExportAsFixedFormat(tempImageFile.Path, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);

                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");
                Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
                await tempImageFile.CopyAsync(outputfolder, "Newpdf.pdf", NameCollisionOption.ReplaceExisting);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (document != null)
                {
                    document.Close();
                }
            }
        }

        //需要付费 测试报错,(可能是文档过大导致)
        public async static void SpireToPDF(StorageFile docfile)
        {
            var resourcepath = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Resources", CreationCollisionOption.OpenIfExists);
            StorageFile wordFile = await docfile.CopyAsync(resourcepath, "Newpdf", NameCollisionOption.ReplaceExisting);
            var tempImageFile = await resourcepath.CreateFileAsync("Newpdf.pdf", CreationCollisionOption.ReplaceExisting);

            Spire.Doc.Document doc = new Spire.Doc.Document();
            doc.LoadFromFile(wordFile.Path);
            doc.SaveToFile(tempImageFile.Path,Spire.Doc.FileFormat.PDF);

            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
            await tempImageFile.CopyAsync(outputfolder, "Newpdf.pdf", NameCollisionOption.ReplaceExisting);
        }

        //需要安装liberoffice 需要libreoffice
        public async static void LiberOfficeTopdf(StorageFile docfile)
        {
            var resourcepath = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Resources", CreationCollisionOption.OpenIfExists);
            StorageFile wordFile = await docfile.CopyAsync(resourcepath, "Newpdf", NameCollisionOption.ReplaceExisting);
            var tempImageFile = await resourcepath.CreateFileAsync("Newpdf.pdf", CreationCollisionOption.ReplaceExisting);
            await tempImageFile.DeleteAsync();

            DocumentConverter documentConverter = new DocumentConverter();
            documentConverter.ConvertToPdf(wordFile.Path,resourcepath+"\\Newpdf.pdf");
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
            tempImageFile =await StorageFile.GetFileFromPathAsync(resourcepath + "\\Newpdf.pdf");
            await tempImageFile.CopyAsync(outputfolder, "Newpdf.pdf", NameCollisionOption.ReplaceExisting);
        }

        //libreoffice 方法不存在 可能缺少依赖
        public async static void officeConverter(StorageFile docfile)
        {
            var resourcepath = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Resources", CreationCollisionOption.OpenIfExists);
            StorageFile wordFile = await docfile.CopyAsync(resourcepath, docfile.Name, NameCollisionOption.ReplaceExisting);
            var tempImageFile = await resourcepath.CreateFileAsync("Newpdf.pdf", CreationCollisionOption.ReplaceExisting);

            OfficeConverter.Converter converter = new Converter();
            converter.UseLibreOffice = true;
            converter.Convert(wordFile.Path, tempImageFile.Path);


            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
            await tempImageFile.CopyAsync(outputfolder, "Newpdf.pdf", NameCollisionOption.ReplaceExisting);
        }

        //openoffice
        public async static void openOfficeTopdf(StorageFile docfile)
        {
            var resourcepath = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Resources", CreationCollisionOption.OpenIfExists);
            StorageFile wordFile = await docfile.CopyAsync(resourcepath, docfile.Name, NameCollisionOption.ReplaceExisting);
            var tempImageFile = await resourcepath.CreateFileAsync("Newpdf.pdf", CreationCollisionOption.ReplaceExisting);

            //Pdfvert.Core.PdfvertService.ConvertFileToPdf(wordFile.Path,tempImageFile.Path);

            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
            if (outputfolder == null) 
                return;
            await tempImageFile.CopyAsync(outputfolder, "Newpdf.pdf", NameCollisionOption.ReplaceExisting);
        }
        #endregion

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".docx");
            openPicker.FileTypeFilter.Add(".pptx");
            openPicker.FileTypeFilter.Add(".doc");
            openPicker.FileTypeFilter.Add(".ppt");
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            FilePath.Text = file.Path;
            //ConverDocToPdf(file);
            //WordToPDF(file);
            //SpireToPDF(file);
           //LiberOfficeTopdf(file);
           // officeConverter(file);
            //openOfficeTopdf(file);
        }

        #region 主题色测试
   

        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            App.ChangeThemes();
        }

        #endregion

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            CloseAllPage();
            ThemeTest.Visibility = Visibility.Visible;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            CloseAllPage();
            ControlTest.Visibility = Visibility.Visible;
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

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            CloseAllPage();
            ScannerTest.Visibility = Visibility.Visible;
        }

        private void NavigationView_SelectionChanged5(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                 contentFrame5.Navigate(typeof(FoldersPage));
            }
            else
            {
                NavigationViewItem selectedItem = (NavigationViewItem)args.SelectedItem;
                string selectedItemTag = ((string)selectedItem.Tag);
                sender.Header = selectedItemTag;
                string pageName = "App1.Control." + selectedItemTag;
                Type pageType = Type.GetType(pageName); 
                contentFrame5.Navigate(pageType);
            }
        }
    }
}
