using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using FolderSyncronizer.ViewModel;
using FolderSyncronizer.Controls;
using FolderSyncronizer.DataModel;
using FolderSyncronizer.Business;

namespace FolderSyncronizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new FolderAndFileSyncViewModel();
            localFolderAndFileViewControl.SelectedItemChanged += LocalFolderAndFileViewControl_SelectedItemChanged;
            remoteFolderAndFileViewControl.SelectedItemChanged += RemoteFolderAndFileViewControl_SelectedItemChanged;
        }


        public FolderAndFileSyncViewModel ViewModel
        {
            get
            {
                return DataContext as FolderAndFileSyncViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private void remoteFolderSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbDialog = new FolderBrowserDialog();
            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.Settings.RemoteFolderPath = fbDialog.SelectedPath;
            }

        }

        private void localFolderSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbDialog = new FolderBrowserDialog();
            if(fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.Settings.LocalFolderPath = fbDialog.SelectedPath;
            }
        }

        #region Tree UI Synchronizer

        #region Selection
        private void RemoteFolderAndFileViewControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // TreeListViewItem treeViewItem = ContainerFromItem(remoteFolderAndFileViewControl.ItemContainerGenerator, remoteFolderAndFileViewControl.SelectedItem);
            var localFolderEquivalent = ViewModel.LocalFileFolderItem[0].GetItem((remoteFolderAndFileViewControl.SelectedItem as FileFolderItem).RelativePath);
            TreeListViewItem treeViewItem = ContainerFromItem(localFolderAndFileViewControl.ItemContainerGenerator, localFolderEquivalent);
            if(treeViewItem != null)
                treeViewItem.IsSelected = true;
        }

        private void LocalFolderAndFileViewControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var remoteFolderEquivalent = ViewModel.RemoteFileFolderItem[0].GetItem((localFolderAndFileViewControl.SelectedItem as FileFolderItem).RelativePath);
            TreeListViewItem treeViewItem = ContainerFromItem(remoteFolderAndFileViewControl.ItemContainerGenerator, remoteFolderEquivalent);
            if(treeViewItem != null)
                treeViewItem.IsSelected = true;
        }

        #endregion

        #region Expand / Collapse
        private void localFolderAndFileViewControl_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandOrCollapse("Local", e.OriginalSource, true);
        }
        private void localFolderAndFileViewControl_Collapsed(object sender, RoutedEventArgs e)
        {
            ExpandOrCollapse("Local", e.OriginalSource, false);
        }
        private void remoteFolderAndFileViewControl_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandOrCollapse("Remote", e.OriginalSource, true);
        }

        private void remoteFolderAndFileViewControl_Collapsed(object sender, RoutedEventArgs e)
        {
            ExpandOrCollapse("Remote", e.OriginalSource, false);
        }

        private void ExpandOrCollapse(string sourceEvent, object originalSource, bool isExpanded)
        {
            if (ViewModel.RemoteFileFolderItem == null || ViewModel.LocalFileFolderItem == null)
                return;

            FileFolderItem eventOrginateditem = ((TreeViewItem)originalSource).DataContext as FileFolderItem;
            FileFolderItem equivalentSibilingFolder = null;
            TreeListViewItem equivalentSibilingTreeViewItem;

            if(sourceEvent == "Local")
            {
                equivalentSibilingFolder = ViewModel.RemoteFileFolderItem[0].GetItem(eventOrginateditem.RelativePath);
                equivalentSibilingTreeViewItem = ContainerFromItem(remoteFolderAndFileViewControl.ItemContainerGenerator, equivalentSibilingFolder);
            }
            else
            {
                equivalentSibilingFolder = ViewModel.LocalFileFolderItem[0].GetItem(eventOrginateditem.RelativePath);
                equivalentSibilingTreeViewItem = ContainerFromItem(localFolderAndFileViewControl.ItemContainerGenerator, equivalentSibilingFolder);
            }

            
            if (equivalentSibilingTreeViewItem != null)
                equivalentSibilingTreeViewItem.IsExpanded = isExpanded;
        }

        #endregion 

        #endregion

        #region Helpers
        private static TreeListViewItem ContainerFromItem(ItemContainerGenerator containerGenerator, object item)
        {
            TreeListViewItem container = (TreeListViewItem)containerGenerator.ContainerFromItem(item);
            if (container != null)
                return container;

            foreach (object childItem in containerGenerator.Items)
            {
                TreeListViewItem parent = containerGenerator.ContainerFromItem(childItem) as TreeListViewItem;
                if (parent == null)
                    continue;

                container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeListViewItem;
                if (container != null)
                    return container;

                container = ContainerFromItem(parent.ItemContainerGenerator, item);
                if (container != null)
                    return container;
            }
            return null;
        }

        #endregion

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            var clickedBtn = sender as System.Windows.Controls.Button;
            string pathToCopy = clickedBtn.Tag as string;
            var remoteItem = ViewModel.RemoteFileFolderItem[0].GetItem(pathToCopy);
            var localItem = ViewModel.LocalFileFolderItem[0].GetItem(pathToCopy);

            FileCopySequencer sequencer = new FileCopySequencer();
            localItem.PrepareFileCopy(remoteItem, sequencer);
            sequencer.OnFileCopyCompleted += Sequencer_OnFileCopyCompleted;
            Task.Run(new Action(sequencer.StartCopy));
        }

        private void Sequencer_OnFileCopyCompleted(FileFolderItem destItem)
        {
            destItem.IsCopyInProgress = false;
            destItem.HasDifference = false;
            System.IO.FileInfo info = new System.IO.FileInfo(destItem.ItemPath);
            destItem.DisplayName = info.Name;
            destItem.LastModifiedTime = info.LastWriteTime;
            destItem.FileSize = info.Length;
        }
    }
}
