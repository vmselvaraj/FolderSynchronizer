using FolderSyncronizer.DataModel;
using RoboSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        private bool isCopyInProgress = false;
        public void StartCopy()
        {
            foreach(var source in itemsToCopy.Keys)
            {
                while(isCopyInProgress)
                {
                    Thread.Sleep(100);
                }

                isCopyInProgress = true;
                var dest = itemsToCopy[source];

                RoboCommand copyCommand = new RoboCommand();
                //events
                copyCommand.OnCopyProgressChanged += CopyCommand_OnCopyProgressChanged;
                // copy options
                copyCommand.CopyOptions.Source = Path.GetDirectoryName(source.ItemPath);
                copyCommand.CopyOptions.Destination = Path.GetDirectoryName(dest.ItemPath);
                copyCommand.CopyOptions.FileFilter = dest.ItemName;
                copyCommand.CopyOptions.CopySubdirectories = false;
                copyCommand.CopyOptions.UseUnbufferedIo = true;

                // select options
                copyCommand.SelectionOptions.OnlyCopyArchiveFilesAndResetArchiveFlag = true;
                // retry options
                copyCommand.RetryOptions.RetryCount = 1;
                copyCommand.RetryOptions.RetryWaitTime = 2;
                Task startTask = copyCommand.Start();
                
                //System.IO.File.Copy(source.ItemPath, dest.ItemPath, true);
                RaiseEvent(dest);
            }
        }

        private void CopyCommand_OnCopyProgressChanged(object sender, CopyProgressEventArgs e)
        {
            if (e.CurrentFileProgress == 100)
                isCopyInProgress = false;
        }

        private void RaiseEvent(FileFolderItem destItemPath)
        {

            if (OnFileCopyCompleted != null)
            {
                var eventListeners = OnFileCopyCompleted.GetInvocationList();

                for (int index = 0; index < eventListeners.Count(); index++)
                {
                    var methodToInvoke = (EventHandler)eventListeners[index];
                    methodToInvoke.BeginInvoke(this, EventArgs.Empty, EndAsyncEvent, null);
                }
            }
        }

        private void EndAsyncEvent(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (EventHandler)ar.AsyncDelegate;

            try
            {
                invokedMethod.EndInvoke(iar);
            }
            catch
            {
                // Handle any exceptions that were thrown by the invoked method
                Console.WriteLine("An event listener couldn't be ended!");
            }
        }

    }
}
