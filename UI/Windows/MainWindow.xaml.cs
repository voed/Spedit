using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Spedit.UI.Components;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;


namespace Spedit.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<EditorElement> EditorsReferences = new List<EditorElement>();
        

		private bool FullyInitialized;

        public MainWindow()
        {
            InitializeComponent();


			ObjectBrowserColumn.Width = new GridLength(Program.Options.Program_ObjectbrowserWidth, GridUnitType.Pixel);
			var heightDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
			heightDescriptor.AddValueChanged(EditorObjectBrowserGrid.ColumnDefinitions[1], EditorObjectBrowserGridRow_WidthChanged);
			FillConfigMenu();
            ChangeObjectBrowserToDirectory(Program.Options.Program_ObjectBrowserDirectory);

            if (Program.Options.LastOpenFiles != null)
            {
                foreach (var t in Program.Options.LastOpenFiles)
                {
                    TryLoadSourceFile(t, false);
                }
            }
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (!args[i].EndsWith("exe"))
                {
                    TryLoadSourceFile(args[i], false, true, (i == 0));
                }
            }
            
			StartBackgroundParserThread();
			FullyInitialized = true;
		}

        public bool TryLoadSourceFile(string filePath, bool UseBlendoverEffect = true, bool TryOpenIncludes = true, bool SelectMe = false)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                string extension = fileInfo.Extension.ToLowerInvariant().Trim('.', ' ');
                if (Program.SupportedExtensions.Contains(extension))
                {
                    string finalPath = fileInfo.FullName;
                    try
                    {
                        File.GetAccessControl(finalPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return false;
                    }
                    EditorElement[] editors = GetAllEditorElements();
                    if (editors != null)
                    {
                        foreach (var editor in editors)
                        {
                            if (editor.FullFilePath == finalPath)
                            {
                                if (SelectMe)
                                {
                                    editor.Parent.IsSelected = true;
                                }
                                return true;
                            }
                        }
                    }
                    AddEditorElement(finalPath, fileInfo.Name, SelectMe);
                    if (TryOpenIncludes && Program.Options.Program_OpenCustomIncludes)
                    {
                        using (var textReader = fileInfo.OpenText())
                        {
                            string source = Regex.Replace(textReader.ReadToEnd(), @"/\*.*?\*/", string.Empty, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
                            Regex regex = new Regex(@"^\s*\#include\s+((\<|"")(?<name>.+?)(\>|""))", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                            MatchCollection mc = regex.Matches(source);
                            for (int i = 0; i < mc.Count; ++i)
                            {
                                try
                                {
                                    string fileName = mc[i].Groups["name"].Value;
                                    if (!(fileName.EndsWith(".inc", StringComparison.InvariantCultureIgnoreCase) ||
                                          fileName.EndsWith(".sma", StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        //TODO get rid of this
                                        fileName += ".inc";

                                    }

                                    fileName = Path.Combine(fileInfo.DirectoryName, fileName);
                                    TryLoadSourceFile(fileName, false,
                                        Program.Options.Program_OpenIncludesRecursively);
                                }
                                catch (PathTooLongException)
                                {
                                    MessageBox.Show($"Error: path '{fileInfo.FullName}' is too long");
                                }
                                catch (Exception)
                                {
                                    //
                                }
                            }
                        }
                    }
                }
                else
                {
                    //TODO sugar it
                    MessageBox.Show("This file type is not supported");
                }
                return true;
            }
            return false;
        }

        public void AddEditorElement(string filePath, string name, bool SelectMe)
        {
            LayoutDocument layoutDocument = new LayoutDocument {Title = name};
            layoutDocument.Closing += layoutDocument_Closing;
            layoutDocument.ToolTip = filePath;
            bool compileChecked = filePath.EndsWith(".sma");
            EditorElement editor = new EditorElement(filePath, compileChecked, this) {Parent = layoutDocument};
            
            layoutDocument.Content = editor;
            EditorsReferences.Add(editor);
            DockingPane.Children.Add(layoutDocument);
            if (SelectMe)
            {
				layoutDocument.IsSelected = true;
            }
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle();
            EditorElement ee = GetCurrentEditorElement();
            ee?.editor.Focus();
        }

        private void DockingManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            if (e.Document.Content is EditorElement element)
            {
                element.Close();
            }
            UpdateWindowTitle();
        }

        private void layoutDocument_Closing(object sender, CancelEventArgs e)
        {
			e.Cancel = true;
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            backgroundParserThread?.Abort();
            parseDistributorTimer?.Stop();
            ServerCheckThread?.Abort(); //a join would not work, so we have to be..forcefully...

            List<string> lastOpenFiles = new List<string>();
            EditorElement[] editors = GetAllEditorElements();
            bool? saveUnsaved = null;
			if (editors != null)
            {
                foreach (var editor in editors)
                {
                    if (File.Exists(editor.FullFilePath))
                    {
                        lastOpenFiles.Add(editor.FullFilePath);
                        if (editor.NeedsSave)
                        {
                            if (saveUnsaved == null)
                            {
                                var result = MessageBox.Show(this, Properties.Resources.SavingUFiles, Properties.Resources.Saving, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                saveUnsaved = (result == MessageBoxResult.Yes);
                            }

                            editor.Close(saveUnsaved.Value, saveUnsaved.Value);
                        }
                        else
                        {
                            editor.Close(false, false);
                        }
                    }
                }
            }
            Program.Options.LastOpenFiles = lastOpenFiles.ToArray();
#if !DEBUG
            if (Program.UpdateStatus.IsAvailable)
            {
                UpdateWindow updateWin = new UpdateWindow(Program.UpdateStatus) { Owner = this };
                updateWin.ShowDialog();
            }
#endif
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Activate();
                Focus();
                bool first = true;
                foreach(string file in files)
                {
                    TryLoadSourceFile(file, first, true, first);
                    first = false;
                }
            }
        }

        public static void ProcessUITasks()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }

        private void ErrorResultGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = (ErrorDataGridRow)ErrorResultGrid.SelectedItem;
            if (row == null)
            {
                return;
            }
            string fileName = row.file;
            EditorElement[] editors = GetAllEditorElements();
            if (editors == null)
            {
                return;
            }
            foreach (var editor in editors)
            {
                if (editor.FullFilePath == fileName)
                {
                    editor.Parent.IsSelected = true;
                    int line = GetLineInteger(row.line);
                    if (line > 0 && line <= editor.editor.LineCount)
                    {
                        var lineObj = editor.editor.Document.Lines[line - 1];
                        editor.editor.ScrollToLine(line - 1);
                        editor.editor.Select(lineObj.Offset, lineObj.Length);
                    }
                }
            }
        }

        private void CloseErrorResultGrid(object sender, RoutedEventArgs e)
        {
            CompileOutputRow.Height = new GridLength(8.0);
        }

		private void EditorObjectBrowserGridRow_WidthChanged(object sender, EventArgs e)
		{
			if (FullyInitialized)
			{
				Program.Options.Program_ObjectbrowserWidth = ObjectBrowserColumn.Width.Value;
			}
		}

		private void UpdateWindowTitle()
        {
            EditorElement ee = GetCurrentEditorElement();
            string outString;
            if (ee == null)
            {
                outString = "SPEdit";
            }
            else
            {
                outString = ee.FullFilePath + " - SPEdit";
            }
            if (ServerIsRunning)
            {
                outString = $"{outString} ({Properties.Resources.ServerRunning})";
            }
            Title = outString;
        }

        private int GetLineInteger(string lineStr)
        {
            int end = 0;
            for (int i = 0; i < lineStr.Length; ++i)
            {
                if (lineStr[i] >= '0' && lineStr[i] <= '9')
                {
                    end = i;
                }
                else
                {
                    break;
                }
            }
            int line;
            if (int.TryParse(lineStr.Substring(0, end + 1), out line))
            {
                return line;
            }
            return -1;
        }

        private ObservableCollection<string> compileButtonDict = new ObservableCollection<string> { Properties.Resources.CompileAll, Properties.Resources.CompileCurr };
        private ObservableCollection<string> actionButtonDict = new ObservableCollection<string> { Properties.Resources.Copy, Properties.Resources.FTPUp, Properties.Resources.StartServer };
        private ObservableCollection<string> findReplaceButtonDict = new ObservableCollection<string> { Properties.Resources.Replace, Properties.Resources.ReplaceAll };


    }
}
