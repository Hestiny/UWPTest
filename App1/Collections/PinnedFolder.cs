using App1.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.Collections
{
    class PinnedFolder : ViewModelBase
    {
        public PinnedFolder()
        {
        }

        /// <summary>
        /// 用来赋值文件的属性值,构造函数无法异步调用
        /// </summary>
        /// <param name="storageItem"></param>
        public async void InitFileAttribute(IStorageItem storageItem)
        {
            StorageItemThumbnail ThumbNavil = null;
            File = storageItem;
            if (IsFile)
            {
                ThumbNavil = await (File as StorageFile).GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.DocumentsView);
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                await RandomAccessStream.CopyAsync(ThumbNavil, randomAccessStream);
                randomAccessStream.Seek(0); 
                ThumbImage.SetSource(randomAccessStream);
            }
        }

        /// <summary>
        /// 赋值文件时更新文件信息
        /// </summary>
        /// <param name="value"></param>
        private void SetFileInfo(IStorageItem value)
        {
            if (value == null)
                Name = "Pinned";
            else
                Name = value.Name;

            if (value is StorageFile)
                IsFile = true;
            else
                IsFile = false;
           
        }

        public bool IsFile { get; set; }

        private IStorageItem _File = null;

        public IStorageItem File
        {
            get { return _File; }
            set
            {
                Set(ref _File, value);
                SetFileInfo(value);
            }
        }

        private BitmapImage _ThumbImage = new BitmapImage();

        public BitmapImage ThumbImage
        {
            get{ return _ThumbImage; }
            set { Set(ref _ThumbImage, value); }
        }

        private string _Name = "";

        public string Name 
        { 
            get{ return _Name; }
            set { Set(ref _Name, value); }
        }
        public enum PinnedType
        {
            Pin,
            Folder,
            File
        }
    }
}
