using App1.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace App1.ViewModel
{
    class FoldersPageViewModel: ViewModelBase
    {

    }

    class PinnedFolderLocalSetting
    {
        const string ContainerName = "PinnedFolderContainer";
        const string PINNED_FOLDER_NAME = "PinnedFolderNumber_";
        const string PINNED_FOLDER_COUNT = "PinnedFolderCount";
        protected ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private ApplicationDataCompositeValue Container;

        public PinnedFolderLocalSetting()
        {
            InitContainer();
        }
        #region ====私有方法====
        private void InitContainer()
        {
            if (localSettings.Values.ContainsKey(ContainerName))
            {
                Container = (ApplicationDataCompositeValue)localSettings.Values[ContainerName];
            }
            else
            {
                Container = new ApplicationDataCompositeValue();
                Container[PINNED_FOLDER_COUNT] = 0;
                localSettings.Values[ContainerName] = Container;
            }
        }

        private int PinedFolderCount
        {
            set
            {
                localSettings.Values[PINNED_FOLDER_COUNT] = value;
            }
            get
            {
                if (localSettings.Values.ContainsKey(PINNED_FOLDER_COUNT))
                {
                    return (int)localSettings.Values[PINNED_FOLDER_COUNT];
                }

                return 0;
            }
        }

        private string GetPinnerFolderToken(int index)
        {
            string settingsName = PINNED_FOLDER_NAME + index;
            if (localSettings.Values.ContainsKey(settingsName))
            {
                return (string)localSettings.Values[settingsName];
            }

            return null;
        }

        private void SetPinnedFolderToken(string token, int index)
        {
            string settingsName = PINNED_FOLDER_NAME + index;
            localSettings.Values[settingsName] = token;
        }

        #endregion


        #region ====公有方法====
     

        public async Task<List<IStorageItem>> GetPinnedFolder()
        {
            if (PinedFolderCount == 0)
                return null;
            List<IStorageItem> items = new List<IStorageItem>();
            try
            {
                for (int index = 0; index <=PinedFolderCount; ++index)
                {
                    string settingsName = PINNED_FOLDER_NAME + index;
                    if (localSettings.Values.ContainsKey(settingsName))
                    {
                        items.Add(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync((string)localSettings.Values[settingsName]));
                    }
                }
            }
            catch (Exception e) { return null; }

            return items;
        }

        public void AddPinnedFolder(StorageFolder storageFolder)
        {
            ++PinedFolderCount;
            string token = StorageApplicationPermissions.FutureAccessList.Add(storageFolder);
            string settingsName = PINNED_FOLDER_NAME + PinedFolderCount;
            localSettings.Values[settingsName] = token;
        }

        /// <summary>
        /// 删除当前的选项,则直接用后面的值覆盖前一项的值
        /// </summary>
        /// <param name="deleteIndex"></param>
        public void DeletePinnedFolder(int deleteIndex)
        {
            --PinedFolderCount;
            for (int index = deleteIndex; index < PinedFolderCount; ++index)
            {
                SetPinnedFolderToken(GetPinnerFolderToken(index + 1), index);
            }
        }

        #endregion


    }
}
