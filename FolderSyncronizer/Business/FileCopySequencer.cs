using FolderSyncronizer.DataModel;
using RoboSharp;
using System;
using System.Collections.Generic;
using System.IO;
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
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 3,
            };
            
           Parallel.ForEach(itemsToCopy.Keys, options, source =>
            {
                var dest = itemsToCopy[source];

                //RoboCopy.Copy(Path.GetDirectoryName(source.ItemPath), Path.GetDirectoryName(dest.ItemPath), dest.ItemName);

                File.Copy(source.ItemPath, dest.ItemPath);
                //Raise Events in Seperate Task
                Action<FileFolderItem> raisedEventAction = new Action<FileFolderItem>(RaiseEvent);
                raisedEventAction.BeginInvoke(dest, null, null);
            });
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
