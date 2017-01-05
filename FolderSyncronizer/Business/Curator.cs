﻿using FolderSyncronizer.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncronizer.Business
{
    public static class Curator
    {
        public static void Curate(FileFolderItem local, FileFolderItem remote)
        {
            if (local != null && remote != null)
            {
                foreach (var remoteItem in remote.Children)
                {
                    var itemInLocal = local.GetItem(remoteItem.RelativePath);
                    if (itemInLocal != null )
                    {
                        if (remoteItem.Type == ItemType.File)
                        {
                            itemInLocal.HasDifference = itemInLocal.FileSize != remoteItem.FileSize && itemInLocal.LastModifiedTime != remoteItem.LastModifiedTime;
                        }
                    }
                    else
                    {
                        var remoteParent = remoteItem.Parent;
                        var localParent = local.GetItem(remoteParent.RelativePath);
                        if (localParent == null)
                            localParent = local;

                        var newFileFolderItem = new FileFolderItem { DisplayName = remoteItem.ItemName, Parent = localParent};
                        localParent.Children.Insert(remote.Children.IndexOf(remoteItem), newFileFolderItem );
                        newFileFolderItem.HasDifference = true;
                    }
                    
                    if(remoteItem.Type == ItemType.Folder)
                        Curate(local, remoteItem);
                }
            }            
        }
    }
}
