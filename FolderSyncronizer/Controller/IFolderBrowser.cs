using FolderSyncronizer.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncronizer.Controller
{
    public interface IFolderBrowser
    {
        FileFolderItem GetFileFolderItem();
    }
}
