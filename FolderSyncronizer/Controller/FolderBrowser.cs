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
        public FolderBrowser(string folderPath)
        {
            FolderPath = folderPath;
        }
        public FileFolderItem GetFileFolderItem()
        {
            FileFolderItem rootDirectoryItem = null;
            DirectoryInfo rootDirectoryInfo = new DirectoryInfo(FolderPath);
            rootDirectoryItem = new FileFolderItem
            {
                ItemPath = FolderPath,
                DisplayName = rootDirectoryInfo.Name,
                Type = ItemType.Folder,
                Parent = null,
            };
            LoadFileFolderItems(rootDirectoryItem, rootDirectoryInfo);
            return rootDirectoryItem;
        }

        private void LoadFileFolderItems(FileFolderItem fileFolderItem, DirectoryInfo directoryToBrowser)
        {
            FileInfo[] files = directoryToBrowser.GetFiles();
            foreach(var fileInfo in files)
            {
                FileFolderItem fileItem = new FileFolderItem();
                fileItem.DisplayName = fileInfo.Name;
                fileItem.Type = ItemType.File;
                fileItem.LastModifiedTime = fileInfo.LastWriteTime;
                fileItem.FileSize = fileInfo.Length;
                fileItem.Parent = fileFolderItem;
                fileFolderItem.Children.Add(fileItem);

            }

            DirectoryInfo[] directories = directoryToBrowser.GetDirectories();
            foreach(var directoryInfo in directories)
            {
                FileFolderItem directoryItem = new FileFolderItem
                {
                    DisplayName = directoryInfo.Name,
                    Type = ItemType.Folder,
                    LastModifiedTime = directoryInfo.LastWriteTime,
                };

                directoryItem.Parent = fileFolderItem;
                fileFolderItem.Children.Add(directoryItem);

                LoadFileFolderItems(directoryItem, directoryInfo);
            }
        }

        
    }
}
