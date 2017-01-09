using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FolderSyncronizer.DataModel
{
    public class Settings : INotifyPropertyChanged
    {
         
        private string m_LocalFolderPath = "..";
        public string LocalFolderPath
        {
            get
            {
                return m_LocalFolderPath;
            }
            set
            {
                m_LocalFolderPath = value;
                OnPropertyChanged("LocalFolderPath");
            }
        }

        private string m_RemoteFolderPath = "..";
        public string RemoteFolderPath
        {
            get
            {
                return m_RemoteFolderPath;
            }
            set
            {
                m_RemoteFolderPath = value;
                OnPropertyChanged("RemoteFolderPath");
            }
        }

        [XmlIgnore]
        public bool IsLocalFolderExists
        {
            get
            {
                return System.IO.Directory.Exists(LocalFolderPath);
            }
        }

        [XmlIgnore]
        public bool IsRemoteFolderExisits
        {
            get
            {
                return System.IO.Directory.Exists(RemoteFolderPath);
            }
        }


        public string FolderExistanceErrorMessage
        {
            get
            {
                string errorMessage = string.Empty;
                if (!IsLocalFolderExists)
                    errorMessage = "Local Folder";

                if (!IsRemoteFolderExisits)
                {
                    errorMessage += !string.IsNullOrEmpty(errorMessage) ? " & " : "";
                    errorMessage += "Remote Folder";
                }

                if(!string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage += " does not exist";
                }

                return errorMessage;
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
