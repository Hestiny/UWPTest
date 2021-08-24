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
using Windows.Storage;
using Aspose.Words;
using Windows.Storage.Pickers;
using LibreOfficeLibrary;
using OfficeConverter;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App1.Control
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WordTopdfPage : Page
    {
        public WordTopdfPage()
        {
            this.InitializeComponent();
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
                await tempImageFile.CopyAsync(outputfolder, "Newpdf.pdf", NameCollisionOption.ReplaceExisting);
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
            doc.SaveToFile(tempImageFile.Path, Spire.Doc.FileFormat.PDF);

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
            documentConverter.ConvertToPdf(wordFile.Path, resourcepath + "\\Newpdf.pdf");
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder outputfolder = await folderPicker.PickSingleFolderAsync();
            tempImageFile = await StorageFile.GetFileFromPathAsync(resourcepath + "\\Newpdf.pdf");
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

    }
}
