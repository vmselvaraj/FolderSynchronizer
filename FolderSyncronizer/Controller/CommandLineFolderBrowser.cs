using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSyncronizer.DataModel;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FolderSyncronizer.Business;

namespace FolderSyncronizer.Controller
{
    public class CommandLineFolderBrowser : IFolderBrowser
    {
        private string FolderPath { get; set; }
        public event BrowsePathChangedDelegate BrowsePatchChanged;

        public CommandLineFolderBrowser(string folderPath)
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
            LoadFileFolderItems(rootDirectoryItem);
            return rootDirectoryItem;
        }

        private void LoadFileFolderItems(FileFolderItem fileFolderItem)
        {
            if (BrowsePatchChanged != null)
                BrowsePatchChanged(fileFolderItem.ItemPath);

            ProcessStartInfo si = new ProcessStartInfo("cmd.exe");
            // Redirect both streams so we can write/read them.
            si.RedirectStandardInput = true;
            si.RedirectStandardOutput = true;
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            // Start the procses.
            Process p = Process.Start(si);
            // Issue the dir command.
            p.StandardInput.WriteLine(string.Format("dir \"{0}\"", fileFolderItem.ItemPath));
            // Exit the application.
            p.StandardInput.WriteLine(@"exit");
            // Read all the output generated from it.
            string output = p.StandardOutput.ReadToEnd();

            ConvertCommandLineOutputToObject(fileFolderItem, output);

            var loop = Parallel.ForEach(fileFolderItem.Children, new ParallelOptions { MaxDegreeOfParallelism = 10 }, child =>
            {
                if (child.Type == ItemType.Folder)
                    LoadFileFolderItems(child);
            });

            //Wait for the loop to be completed
            while (!loop.IsCompleted) { }
        }

        private void ConvertCommandLineOutputToObject(FileFolderItem parent, string output)
        {
            using (var reader = new StreamReader(GenerateStreamFromString(output)))
            {
                while(!reader.EndOfStream)
                {
                    string outputLine = reader.ReadLine();

                    DateTime? dateTime = CommandLineDirOutputParser.GetDateTimeString(ref outputLine);
                    if (dateTime == null)
                        continue;
                    ItemType itemType = CommandLineDirOutputParser.IsDirectory(ref outputLine) ? ItemType.Folder : ItemType.File;
                    long fileSizeInBytes = 0;
                    if (itemType == ItemType.File)
                        fileSizeInBytes = CommandLineDirOutputParser.GetFileSize(ref outputLine);

                    string itemName = outputLine.Trim();

                    if (itemName == "." || itemName == "..")
                        continue;

                    FileFolderItem item = new FileFolderItem
                    {
                        DisplayName = itemName,
                        Type = itemType,
                        Parent = parent,
                        FileSize = fileSizeInBytes,
                        LastModifiedTime = dateTime.Value,
                    };

                    parent.Children.Add(item);
                }
            }
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


    }
}
