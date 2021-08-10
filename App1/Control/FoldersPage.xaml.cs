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
        public  List<IStorageItem> Items;
        public  ObservableCollection<object> Breadcrumbs =new ObservableCollection<object>();

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


        private void InitializeView()
        {
            // Start with Pictures and Music libraries.
            Items = new List<IStorageItem>();
            Items.Add(KnownFolders.PicturesLibrary);
            Items.Add(KnownFolders.MusicLibrary);
            FolderView.ItemsSource = Items;

            Breadcrumbs.Clear();
            Breadcrumbs.Add(new Crumb("Home", null));
        }

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

        private async Task GetFolderItems(StorageFolder folder)
        {
            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();
            FolderView.ItemsSource = itemsList;
        }

        private async void FolderListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is StorageFolder)
            {
                StorageFolder folder = e.ClickedItem as StorageFolder;
                await GetFolderItems(folder);
                Breadcrumbs.Add(new Crumb(folder.DisplayName, folder));
            }
        }
    }
}
