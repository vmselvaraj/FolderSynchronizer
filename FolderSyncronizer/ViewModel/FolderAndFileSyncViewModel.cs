using FolderSyncronizer.Business;
using FolderSyncronizer.Controller;
using FolderSyncronizer.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncronizer.ViewModel
{
    public class FolderAndFileSyncViewModel : INotifyPropertyChanged
    {
        public Settings Settings { get; set; }

        private ObservableCollection<FileFolderItem> m_LocalFileFolderItem = null;
        public  ObservableCollection<FileFolderItem> LocalFileFolderItem
        {
            get
            {
                return m_LocalFileFolderItem;
            }
            set
            {
                m_LocalFileFolderItem = value;
                OnPropertyChanged("LocalFileFolderItem");
            }
        }


        private ObservableCollection<FileFolderItem> m_RemoteFileFolderItem = null;
        public ObservableCollection<FileFolderItem> RemoteFileFolderItem
        {
            get
            {
                return m_RemoteFileFolderItem;
            }
            set
            {
                m_RemoteFileFolderItem = value;
                OnPropertyChanged("RemoteFileFolderItem");
            }
        }

        public FolderAndFileSyncViewModel()
        {
            Settings = SettingsIO.LoadSettings();
            Settings.PropertyChanged += Settings_PropertyChanged;
            LoadFolderTree();
            ValidateConfiguration();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateConfiguration();
            SettingsIO.PersistSettings(Settings);
            LoadFolderTree(e.PropertyName);
        }

        private void LoadFolderTree(string path = null)
        {
            if(path== null || path == "LocalFolderPath")
            {
                IFolderBrowser browser = new FolderBrowser(Settings.LocalFolderPath);
                LocalFileFolderItem = new ObservableCollection<FileFolderItem> { browser.GetFileFolderItem() };
                if (RemoteFileFolderItem != null && RemoteFileFolderItem.Count() == 1)
                    Curator.Curate(LocalFileFolderItem[0], RemoteFileFolderItem[0]);
            }
            
            if(path == null || path == "RemoteFolderPath")
            {
                IFolderBrowser browser = new FolderBrowser(Settings.RemoteFolderPath);
                RemoteFileFolderItem = new ObservableCollection<FileFolderItem> { browser.GetFileFolderItem() };
                if (LocalFileFolderItem != null && LocalFileFolderItem.Count() == 1)
                    Curator.Curate(LocalFileFolderItem[0], RemoteFileFolderItem[0]);
            }
                
        }

        #region INotifyPropertyChanged Implementation
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Status Managers

        private bool m_FolderAndFileViewControVisibility = false;
        public bool FolderAndFileViewControVisibility
        {
            get
            {
                return m_FolderAndFileViewControVisibility;
            }
            set
            {
                m_FolderAndFileViewControVisibility = value;
                OnPropertyChanged("FolderAndFileViewControVisibility");
            }
        }

        private string m_Status = string.Empty;
        public string Status
        {
            get
            {
                return m_Status;
            }
            set
            {
                m_Status = value;
                OnPropertyChanged("Status");

            }
        }
        public bool Validate()
        {
            string status = string.Empty;

            if (Settings.LocalFolderPath == "..")
                status = "Select Local Folder Path";

            if(Settings.RemoteFolderPath == "..")
            {
                if (string.IsNullOrEmpty(status))
                    status = "Select Remote Folder Path";
                else
                    status += " and Remote Folder Path";
            }

            Status = status;
            return !String.IsNullOrEmpty(Status);
        }

        public void ValidateConfiguration()
        {
            if(Validate())
            {
                FolderAndFileViewControVisibility = false;
            }
            else
            {
                FolderAndFileViewControVisibility = true;
            }
        }

        #endregion 
    }
}
