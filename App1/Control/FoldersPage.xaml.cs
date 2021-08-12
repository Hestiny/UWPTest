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
        public ObservableCollection<object> Items =new ObservableCollection<object>();
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

        private ListViewSelectionMode _SelectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode SelectionMode
        {
            get { return _SelectionMode; }
            set { _SelectionMode = value; }
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
            Items.Clear();
            Items.Add(pinnedFolder);

            List<IStorageItem> storageItems = await pinnedFolderLocalSetting.GetPinnedFolder();
            var Flodertems = GetPinnedFolderList(storageItems);
            UpdateItems(Flodertems);

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (itemsWrapGrid.MaximumRowsOrColumns == -1)
                itemsWrapGrid.MaximumRowsOrColumns = 1;
            else
                itemsWrapGrid.MaximumRowsOrColumns = -1;
        }

        private ItemsWrapGrid itemsWrapGrid;

        private void itemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            itemsWrapGrid = sender as ItemsWrapGrid;
            itemsWrapGrid.MaximumRowsOrColumns = -1;

        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (FolderView.SelectedItems.Count == 0 || Breadcrumbs.Count != 1)
                return;
            foreach (PinnedFolder item in FolderView.SelectedItems)
            {
                int index = Items.IndexOf(item);
                if (index != -1)
                {
                    pinnedFolderLocalSetting.DeletePinnedFolder(index);
                    Items.Remove(item);
                }
            }

        }

        private void SelectMode_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionMode == ListViewSelectionMode.None)
                SelectionMode = ListViewSelectionMode.Multiple;
            else
                SelectionMode = ListViewSelectionMode.None;
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

        private void UpdateItems(List<object> list)
        {
            if (list == null)
                return;
            foreach(var item in list)
            {
                Items.Add(item);
            }
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
            Items.Clear();
            UpdateItems(pinnedList);
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
                pinnedFolderLocalSetting.AddPinnedFolder(folder);
            }
        }


    }
}
