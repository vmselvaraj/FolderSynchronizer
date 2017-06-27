using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSyncronizer.DataModel;
using System.IO;

namespace FolderSyncronizer.Controller
{
    public class FolderBrowser : IFolderBrowser
    {
        private string FolderPath { get; set; }
        public event Business.BrowsePathChangedDelegate BrowsePatchChanged;

        public FolderBrowser(string folderPath)
        {
            FolderPath = folderPath;
        }
        public FileFolderItem GetFileFolderItem()
        {
            FileFolderItem rootDirectoryItem = null;
            rootDirectoryItem = new FileFolderItem
            {
                ItemPath = FolderPath,
                DisplayName = Path.GetFileName(FolderPath),
                Type = ItemType.Folder,
                Parent = null,
            };
            LoadFileFolderItems(rootDirectoryItem, FolderPath);
            return rootDirectoryItem;
        }

        private void LoadFileFolderItems(FileFolderItem fileFolderItem, string directoryToBrowse)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 2
            };

            string[] files = Directory.GetFiles(directoryToBrowse);
            var fileLoopResult = Parallel.ForEach(files, parallelOptions, file => {
                var fileInfo = new FileInfo(file);
                FileFolderItem fileItem = new FileFolderItem();
                fileItem.DisplayName = fileInfo.Name;
                fileItem.Type = ItemType.File;
                fileItem.LastModifiedTime = fileInfo.LastWriteTime;
                fileItem.FileSize = fileInfo.Length;
                fileItem.Parent = fileFolderItem;
                AddChid(fileFolderItem, fileItem);
            });
            
            
            string[] directories = System.IO.Directory.GetDirectories(directoryToBrowse);
            var folderLoopResult = Parallel.ForEach(directories, parallelOptions, directory => {
                FileFolderItem directoryItem = new FileFolderItem
                {
                    DisplayName = Path.GetFileName(directory),
                    Type = ItemType.Folder,
                };

                directoryItem.Parent = fileFolderItem;
                AddChid(fileFolderItem, directoryItem);
                LoadFileFolderItems(directoryItem, directory);
            });

            while(!fileLoopResult.IsCompleted || !folderLoopResult.IsCompleted) { } //Wait till both the results completed.
        }

        private void AddChid(FileFolderItem parent, FileFolderItem item)
        {
            lock (parent)
            {
                parent.Children.Add(item);
            }
        }


        
    }
}
