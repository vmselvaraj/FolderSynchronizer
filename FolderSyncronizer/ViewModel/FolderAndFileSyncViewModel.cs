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
        public event BrowsePathChangedDelegate BrowsePatchChanged;

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
            LoadFolderTree();
        }

        private void LoadFolderTree()
        {
            if (string.IsNullOrEmpty(Settings.LocalFolderPath) ||
                string.IsNullOrEmpty(Settings.RemoteFolderPath) ||
                Settings.LocalFolderPath == ".." ||
                Settings.RemoteFolderPath == "..")
                return;


            ShowProgressBar = true;
            FolderAndFileViewControVisibility = false;

            Status = string.Empty;
            if (!string.IsNullOrEmpty(Settings.FolderExistanceErrorMessage))
            {
                Status = Settings.FolderExistanceErrorMessage;
                ShowProgressBar = false;
                return;
            }
           
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
            bg.ProgressChanged += Bg_ProgressChanged;
            bg.DoWork += Bg_DoWork;
            bg.RunWorkerAsync(Settings);
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            
            var settings = e.Argument as Settings;

            DateTime startTime = DateTime.Now;
            worker.ReportProgress(0, "Generating Local Folder Map");
            FileFolderItem local = GetFileFolderItem(settings.LocalFolderPath);
            System.Diagnostics.Debug.WriteLine("Local Fetch: " + DateTime.Now.Subtract(startTime).Milliseconds);

            startTime = DateTime.Now;
            worker.ReportProgress(0, "Generating Remote Folder Map");
            FileFolderItem remote = GetFileFolderItem(settings.RemoteFolderPath);
            System.Diagnostics.Debug.WriteLine("Remote Fetch: " + DateTime.Now.Subtract(startTime).Milliseconds);

            worker.ReportProgress(0,"Curating...");

            local.SortAlphabetically();
            remote.SortAlphabetically();
            Curator.Curate(local, remote);
            Curator.RemoveNonExistingLocalItem(local);
            worker.ReportProgress(0, "");
            e.Result = new object[] { local, remote };
        }

        private void Bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Status = e.UserState.ToString();
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as object[];
            LocalFileFolderItem = new ObservableCollection<FileFolderItem> { result[0] as FileFolderItem };
            RemoteFileFolderItem = new ObservableCollection<FileFolderItem> { result[1] as FileFolderItem };
            FolderAndFileViewControVisibility = true;
            ShowProgressBar = false;
        }

        private FileFolderItem GetFileFolderItem(string path)
        {
            IFolderBrowser browser = new CommandLineFolderBrowser(path);
            browser.BrowsePatchChanged += browser_BrowsePatchChanged;
            return browser.GetFileFolderItem();
        }

        void browser_BrowsePatchChanged(string path)
        {
            if (BrowsePatchChanged != null)
                BrowsePatchChanged(path);
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

        private bool m_ShowProgressBar = false;
        public bool ShowProgressBar
        {
            get
            {
                return m_ShowProgressBar;
            }
            set
            {
                m_ShowProgressBar = value;
                OnPropertyChanged("ShowProgressBar");
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

            if(!string.IsNullOrEmpty(status))
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
