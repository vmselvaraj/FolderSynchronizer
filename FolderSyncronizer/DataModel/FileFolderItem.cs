using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncronizer.DataModel
{
    public class FileFolderItem : INotifyPropertyChanged 
    {
        #region Properties

        private string m_DisplayName;
        public string DisplayName
        {
            get
            {
                return m_DisplayName;
            }

            set
            {
                m_DisplayName = value;
                ItemName = value;
                OnPropertyChanged("DisplayName");
            }
        }


        private string m_ItemPath;
        public string ItemPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_ItemPath) && Parent != null)
                    m_ItemPath = System.IO.Path.Combine(Parent.ItemPath, ItemName);

                return m_ItemPath;
            }
            set
            {
                m_ItemPath = value;
                OnPropertyChanged("ItemPath");
            }
        }

        private string m_ItemName = string.Empty;
        public string ItemName
        {
            get
            {
                return m_ItemName;
            }
            set
            {
                m_ItemName = value;
                OnPropertyChanged("ItemName");
            }
        }

        private DateTime? m_LastModifiedTime = null;
        public DateTime? LastModifiedTime
        {
            get
            {
                return m_LastModifiedTime;
            }
            set
            {
                m_LastModifiedTime = value;
                OnPropertyChanged("LastModifiedTime");
            }
        }

        private long? m_FileSize = null;
        public long? FileSize
        {
            get
            {
                if (Type == ItemType.Folder)
                {
                    m_FileSize = 0;
                    foreach (var child in Children)
                    {
                        if (child.FileSize != null)
                            m_FileSize += child.FileSize;
                    }
                }
                return m_FileSize;
            }
            set
            {
                m_FileSize = value;
                OnPropertyChanged("FileSize");
            }
        }

        public ItemType Type { get; set; }

        public FileFolderItem Parent { get; set; }

        private bool m_HasDifference = false;
        public bool HasDifference
        {
            get
            {
                foreach (var child in Children)
                {
                    m_HasDifference = child.HasDifference;
                    if (m_HasDifference)
                        break;
                }
                
                return m_HasDifference;
            }
            set
            {
                m_HasDifference = value;
                OnPropertyChanged("HasDifference");
                OnPropertyChanged("ShowEqual"); // We raise it here because Is Equal is derived from this proeprty.
            }

        }

        private bool m_PresentInServer = false;
        public bool PresentInServer
        {
            get
            {
                return m_PresentInServer;
            }
            set
            {
                m_PresentInServer = value;
            }
        }

        public bool ShowEqual
        {
            get
            {
                return PresentInServer && IsEqual;
            }
        }

        public bool IsEqual
        {
            get
            {
                return !HasDifference;
            }
        }

        public string RelativePath
        {
            get
            {
                string relativePath = "root";
                if (Parent != null)
                {
                    relativePath = Parent.RelativePath + "\\" + ItemName;
                }

                return relativePath;
            }
        }


        private ObservableCollection<FileFolderItem> m_Children = null;
        public ObservableCollection<FileFolderItem> Children
        {
            get
            {
                return m_Children;
            }
            set
            {
                m_Children = value;
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChaged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion 

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (FileFolderItem newItem in e.NewItems)
                {
                    newItem.PropertyChanged += NewItem_PropertyChanged;
                }
            }
        }

        private void NewItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public FileFolderItem()
        {
            Type = ItemType.Folder;
            m_Children = new ObservableCollection<FileFolderItem>();
            m_Children.CollectionChanged += Children_CollectionChanged;
        }


        #region Find
        public FileFolderItem GetItem(string relativePath)
        {
            FileFolderItem matchedItem = null;

            if(this.RelativePath.ToLower() == relativePath.ToLower())
            {
                return this;
            }
            else
            {
                foreach(var child in Children)
                {
                    matchedItem = child.GetItem(relativePath);
                    if (matchedItem != null)
                        break;
                }
            }

            return matchedItem;
        }

        #endregion

        #region Copy Function

        private bool m_IsCopyInProgress = false;
        public bool IsCopyInProgress
        {
           get
            {
                foreach (var child in Children)
                {
                    m_IsCopyInProgress = child.IsCopyInProgress;
                    if (!m_IsCopyInProgress)
                        break;
                }

                return m_IsCopyInProgress;
            }
            set
            {
                m_IsCopyInProgress = value;
                OnPropertyChanged("IsCopyInProgress");
            }
        }

    
        public void Copy(FileFolderItem remote)
        {
            if (this.HasDifference)
            {
                if (this.Type == ItemType.File && !this.IsCopyInProgress)
                {
                    Parent.CreateIfNotExist();
                    BackgroundWorker copyWorker = new BackgroundWorker();
                    copyWorker.RunWorkerCompleted += CopyWorker_RunWorkerCompleted;
                    copyWorker.DoWork += CopyWorker_DoWork;
                    this.IsCopyInProgress = true;
                    this.HasDifference = false;
                    copyWorker.RunWorkerAsync(remote.ItemPath);
                }
                else
                {
                    CreateIfNotExist();
                    foreach (var child in remote.Children)
                    {
                        var localItem = this.GetItem(child.RelativePath);
                        if (localItem != null)
                            localItem.Copy(child);
                    }
                }
            }
        }

        public void CreateIfNotExist()
        {
            //Return if Folder Exists Already
            if (System.IO.Directory.Exists(ItemPath))
                return;

            //Check for the Parent directory first
            if(this.Parent!= null)
                this.Parent.CreateIfNotExist();

            //Create the folder
            System.IO.Directory.CreateDirectory(ItemPath);
            HasDifference = false;
        }

        public void SortAlphabetically()
        {
            this.Children = new ObservableCollection<FileFolderItem>(this.Children.OrderBy(x => x.ItemName).OrderBy(x => x.Type.ToString()));
            m_Children.CollectionChanged += Children_CollectionChanged;   //Re-Subscribe
            var loopresult = Parallel.ForEach(Children, child =>
            {
                child.SortAlphabetically();
            });

            while (!loopresult.IsCompleted) { }; //Wait till all items are iterated
        }


        private void CopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsCopyInProgress = false;
            this.HasDifference = false;
            System.IO.FileInfo info = new System.IO.FileInfo(this.ItemPath);
            this.DisplayName = info.Name;
            this.LastModifiedTime = info.LastWriteTime;
            this.FileSize = info.Length;
        }

        private void CopyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string remotePath = e.Argument as string;
            try
            {
                System.IO.File.Copy(remotePath, this.ItemPath, true);
            }
            catch
            {

            }
        }

        #endregion


        public override string ToString()
        {
            return ItemName;
        }

    }
}
