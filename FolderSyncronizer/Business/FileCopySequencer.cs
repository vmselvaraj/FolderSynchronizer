using FolderSyncronizer.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncronizer.Business
{
    public delegate void OnFileCopyCompletedDelegate(FileFolderItem destItem);
    public class FileCopySequencer
    {
        public event OnFileCopyCompletedDelegate OnFileCopyCompleted = null;
        //Key: Remote Value: Local
        private Dictionary<FileFolderItem, FileFolderItem> itemsToCopy = null;
        public FileCopySequencer()
        {
            itemsToCopy = new Dictionary<FileFolderItem, FileFolderItem>();
        }

        public void Add(FileFolderItem sourceItem, FileFolderItem destItem)
        {
            itemsToCopy.Add(sourceItem, destItem);
        }

        public void StartCopy()
        {
            foreach(var source in itemsToCopy.Keys)
            {
                var dest = itemsToCopy[source];
                System.IO.File.Copy(source.ItemPath, dest.ItemPath, true);


                //Raise Events in Seperate Task
                Action<FileFolderItem> raisedEventAction = new Action<FileFolderItem>(RaiseEvent);
                raisedEventAction.BeginInvoke(dest, null, null);
            }
        }

        private void RaiseEvent(FileFolderItem destItemPath)
        {
            if (OnFileCopyCompleted != null)
            {
                OnFileCopyCompleted(destItemPath);
            }
        }
    }
}
