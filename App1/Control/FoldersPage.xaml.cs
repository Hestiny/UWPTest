using App1.Collections;
using App1.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace App1.Control
{
    public sealed partial class FoldersPage : Page
    {
        public  List<object> Items;
        public  ObservableCollection<object> Breadcrumbs =new ObservableCollection<object>();
        private PinnedFolderLocalSetting pinnedFolderLocalSetting =new PinnedFolderLocalSetting();

        public readonly struct Crumb
        {
            public Crumb(String label, object data)
            {
                Label = label;
                Data = data;
            }
            public string Label { get; }
            public object Data { get; }
            public override string ToString() => Label;
        }

        public FoldersPage()
        {
            this.InitializeComponent();
            InitializeView();
        }


        private async void InitializeView()
        {
            // Start with Pictures and Music libraries.
            PinnedFolder pinnedFolder = new PinnedFolder();
            pinnedFolder.InitFileAttribute(null);
            Items = new List<object>();
            Items.Add(pinnedFolder);

            List<IStorageItem> storageItems = await pinnedFolderLocalSetting.GetPinnedFolder();
            var Flodertems = GetPinnedFolderList(storageItems);
            if (Flodertems != null)
                Items.AddRange(Flodertems);
            FolderView.ItemsSource = Items; 

            Breadcrumbs.Clear();
            Breadcrumbs.Add(new Crumb("Home", null));
        }

        #region ====界面事件====
        private async void FolderBreadcrumbBar_ItemClicked(Microsoft.UI.Xaml.Controls.BreadcrumbBar sender, Microsoft.UI.Xaml.Controls.BreadcrumbBarItemClickedEventArgs args)
        {
            // Don't process last index (current location)
            if (args.Index < Breadcrumbs.Count - 1)
            {
                // Home is special case.
                if (args.Index == 0)
                {
                    InitializeView();
                }
                // Go back to the clicked item.
                else
                {
                    var crumb = (Crumb)args.Item;
                    await GetFolderItems((StorageFolder)crumb.Data);

                    // Remove breadcrumbs at the end until 
                    // you get to the one that was clicked.
                    while (Breadcrumbs.Count > args.Index + 1)
                    {
                        Breadcrumbs.RemoveAt(Breadcrumbs.Count - 1);
                    }
                }
            }
        }

        private async void FolderListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PinnedFolder pinnedFolder = e.ClickedItem as PinnedFolder;
           
            if (pinnedFolder.File != null )
            {
                if (pinnedFolder.IsFile)
                    return;

                StorageFolder folder = pinnedFolder.File as StorageFolder;
                await GetFolderItems(folder);
                Breadcrumbs.Add(new Crumb(folder.DisplayName, folder));
            }
            else
            {
                OpenFolderPicker();
            }
        }

        #endregion

        private List<object> GetPinnedFolderList(List<IStorageItem> storageItems)
        {
            if (storageItems == null)
                return null;

            List<object> list = new List<object>();
            foreach(var item in storageItems)
            {
                PinnedFolder pinnedFolder = new PinnedFolder();
                pinnedFolder.InitFileAttribute(item);
                list.Add(pinnedFolder);
            }
            return list;
        }

        /// <summary>
        /// 获取新的文件路径下的所有文件
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private async Task GetFolderItems(StorageFolder folder)
        {
            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();
            var pinnedList = GetPinnedFolderList(itemsList.ToList());
            FolderView.ItemsSource = pinnedList;
        }

        private bool _FilePickerOpen = false;

        /// <summary>
        /// 打开文件拾取器
        /// </summary>
        private async void OpenFolderPicker()
        {
            if (_FilePickerOpen)
            {
                return;
            }
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            StorageFolder folder = null;
            _FilePickerOpen = true;
            folderPicker.FileTypeFilter.Add("*");
            try
            {
                // apparently, this sometimes throws a System.Exception "Element not found" for no apparent reason. We want to catch that.
                folder = await folderPicker.PickSingleFolderAsync();
            }
            catch (Exception e)
            {
            }
            finally
            {
                _FilePickerOpen = false;
            }

            AddPinFolder(folder);
        }

        /// <summary>
        /// 将文件夹加入到固定文件列表
        /// </summary>
        /// <param name="folder"></param>
        private void AddPinFolder(StorageFolder folder)
        {
            if (folder != null)
            {
                Items.Add(folder);
                List<object> list= new List<object>();
                list.AddRange(Items);
                FolderView.ItemsSource = list;
                pinnedFolderLocalSetting.AddPinnedFolder(folder);
            }
        }


    }
}
