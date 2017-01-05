using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
