using Spedit.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Spedit.Interop;

namespace Spedit.UI
{
	public partial class MainWindow
	{
		private string CurrentObjectBrowserDirectory = string.Empty;
		private void TreeViewOBItem_Expanded(object sender, RoutedEventArgs e)
		{
			object source = e.Source;
			if (source is TreeViewItem item)
			{
                ObjectBrowserTag itemInfo = (ObjectBrowserTag)item.Tag;
				if (itemInfo.Kind == ObjectBrowserItemKind.Directory)
				{
					if (!Directory.Exists(itemInfo.Value))
					{
						return;
					}
					using (Dispatcher?.DisableProcessing())
					{
						item.Items.Clear();
						List<TreeViewItem> newItems = BuildDirectoryItems(itemInfo.Value);
						foreach (var i in newItems)
						{
							item.Items.Add(i);
						}
					}
				}
			}
		}

		private void TreeViewOBItemParentDir_DoubleClicked(object sender, RoutedEventArgs e)
		{
			DirectoryInfo currentInfo = new DirectoryInfo(CurrentObjectBrowserDirectory);
			DirectoryInfo parentInfo = currentInfo.Parent;
			if (parentInfo != null)
			{
				if (parentInfo.Exists)
				{
					ChangeObjectBrowserToDirectory(parentInfo.FullName);
					return;
				}
			}
			ChangeObjectBrowserToDrives();
		}

		private void TreeViewOBItemFile_DoubleClicked(object sender, RoutedEventArgs e)
		{
			if (sender is TreeViewItem item)
			{
                ObjectBrowserTag itemInfo = (ObjectBrowserTag)item.Tag;
				if (itemInfo.Kind == ObjectBrowserItemKind.File)
				{
					TryLoadSourceFile(itemInfo.Value, true, false, true);
				}
			}
		}

		private void SelectOBFile(object sender, RoutedEventArgs e)
		{
            EditorElement ee = GetCurrentEditorElement();
				if (ee != null)
				{
					FileInfo fInfo = new FileInfo(ee.FullFilePath);
					ChangeObjectBrowserToDirectory(fInfo.DirectoryName);
				}
                //ObjectBrowserButtonHolder.SelectedIndex = -1;
			
		}
		private void SelectOBConfig(object sender, RoutedEventArgs e)
		{

                var cc = Program.ConfigList.Current;
				if (cc.SMDirectories.Length > 0)
				{
					ChangeObjectBrowserToDirectory(cc.SMDirectories[0]);
				}
        }

		private void SelectOBItem(object sender, RoutedEventArgs e)
		{

				object objectBrowserSelectedItem = ObjectBrowser.SelectedItem;
				if (objectBrowserSelectedItem is TreeViewItem item)
				{
                    ObjectBrowserTag itemInfo = (ObjectBrowserTag)item.Tag;
					if (itemInfo.Kind == ObjectBrowserItemKind.Directory)
					{
						ChangeObjectBrowserToDirectory(itemInfo.Value);
					}
					else if (itemInfo.Kind == ObjectBrowserItemKind.ParentDirectory)
					{
						DirectoryInfo currentInfo = new DirectoryInfo(CurrentObjectBrowserDirectory);
						DirectoryInfo parentInfo = currentInfo.Parent;
						if (parentInfo != null)
						{
							if (parentInfo.Exists)
							{
								ChangeObjectBrowserToDirectory(parentInfo.FullName);
								return;
							}
						}
						ChangeObjectBrowserToDrives();
					}
				}
                //ObjectBrowserButtonHolder.SelectedIndex = -1;
			
		}


		private void ChangeObjectBrowserToDirectory(string dir)
		{
			if (string.IsNullOrWhiteSpace(dir))
            {
                Config cc = Program.ConfigList.Current;
				if (cc.SMDirectories.Length > 0)
				{
					dir = cc.SMDirectories[0];
				}
			}
			else if (dir == "0:")
			{
				ChangeObjectBrowserToDrives();
				return;
			}
			if (!Directory.Exists(dir))
			{
				dir = Environment.CurrentDirectory;
			}
            try
            {
                Directory.GetAccessControl(dir);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
			CurrentObjectBrowserDirectory = dir;
			Program.Options.Program_ObjectBrowserDirectory = CurrentObjectBrowserDirectory;

			using (Dispatcher?.DisableProcessing())
			{
				ObjectBrowserDirBlock.Text = dir;
				ObjectBrowser.Items.Clear();
				TreeViewItem parentDirItem = new TreeViewItem() {
					Header = "..",
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.ParentDirectory }
				};
				parentDirItem.MouseDoubleClick += TreeViewOBItemParentDir_DoubleClicked;
				ObjectBrowser.Items.Add(parentDirItem);
				List<TreeViewItem> newItems = BuildDirectoryItems(dir);
				foreach (var item in newItems)
				{
					ObjectBrowser.Items.Add(item);
				}
			}
		}

		private void ChangeObjectBrowserToDrives()
		{
			Program.Options.Program_ObjectBrowserDirectory = "0:";
			DriveInfo[] drives = DriveInfo.GetDrives();
			using (Dispatcher?.DisableProcessing())
			{
				ObjectBrowserDirBlock.Text = string.Empty;
				ObjectBrowser.Items.Clear();
				foreach (var dInfo in drives)
				{
					if (dInfo.IsReady && (dInfo.DriveType == DriveType.Fixed || dInfo.DriveType == DriveType.Removable))
                    {

                            var tvi = new TreeViewItem()
                            {
                                Header = BuildTreeViewItemContent(dInfo.Name, "iconmonstr-folder-13-16.png"),
                                Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.Directory, Value = dInfo.RootDirectory.FullName }
                            };
                            tvi.Items.Add("...");
                            ObjectBrowser.Items.Add(tvi);

                    }
				}
			}
		}

		private List<TreeViewItem> BuildDirectoryItems(string dir)
		{
			List<TreeViewItem> itemList = new List<TreeViewItem>();
			string[] spFiles = Directory.GetFiles(dir, "*.sma", SearchOption.TopDirectoryOnly);
			string[] incFiles = Directory.GetFiles(dir, "*.inc", SearchOption.TopDirectoryOnly);
            //TODO: add cfg and ini files
			string[] directories = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
			foreach (string d in directories)
			{
				DirectoryInfo dInfo = new DirectoryInfo(d);
				if (!dInfo.Exists)
				{
					continue;
				}
                try
                {
                    dInfo.GetAccessControl();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
				var tvi = new TreeViewItem()
				{
					Header = BuildTreeViewItemContent(dInfo.Name, "iconmonstr-folder-13-16.png"),
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.Directory, Value = dInfo.FullName }
				};
				tvi.Items.Add("...");
				itemList.Add(tvi);
			}
			foreach (string f in spFiles)
			{
				FileInfo fInfo = new FileInfo(f);
				if (!fInfo.Exists)
				{
					continue;
				}
				var tvi = new TreeViewItem()
				{
					Header = BuildTreeViewItemContent(fInfo.Name, "iconmonstr-file-5-16.png"),
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.File, Value = fInfo.FullName }
				};
				tvi.MouseDoubleClick += TreeViewOBItemFile_DoubleClicked;
				itemList.Add(tvi);
			}
			foreach (string f in incFiles)
			{
				FileInfo fInfo = new FileInfo(f);
				if (!fInfo.Exists)
				{
					continue;
				}
				var tvi = new TreeViewItem()
				{
					Header = BuildTreeViewItemContent(fInfo.Name, "iconmonstr-file-8-16.png"),
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.File, Value = fInfo.FullName }
				};
				tvi.MouseDoubleClick += TreeViewOBItemFile_DoubleClicked;
				itemList.Add(tvi);
			}
			return itemList;
		}

		private object BuildTreeViewItemContent(string headerString, string iconFile)
		{
            StackPanel stack = new StackPanel {Orientation = Orientation.Horizontal};
            Image image = new Image();
			string uriPath = $"/Spedit;component/Resources/{iconFile}";
			image.Source = new BitmapImage(new Uri(uriPath, UriKind.Relative));
			image.Width = 16;
			image.Height = 16;
            TextBlock lbl = new TextBlock {Text = headerString, Margin = new Thickness(2.0, 0.0, 0.0, 0.0)};
            stack.Children.Add(image);
			stack.Children.Add(lbl);
			return stack;
		}

		private class ObjectBrowserTag
		{
			public ObjectBrowserItemKind Kind;
			public string Value;
		}

		private enum ObjectBrowserItemKind
		{
			ParentDirectory,
			Directory,
			File
		}
	}
}
